using Microsoft.EntityFrameworkCore;
using PmEngine.Core.Enums;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PmEngine.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseClasses
{
    /// <summary>
    /// Базовый контекст БД для PmEngine.Core
    /// Может быть расширен в дочерних библиотеках через наследование
    /// </summary>
    public class PMEContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserLocalEntity> UserLocals { get; set; }
        public DbSet<UserPermissionEntity> UserPermissions { get; set; }

        protected PmConfig? _configurator;
        protected IServiceProvider? _serviceProvider;

        /// <summary>
        /// Контекст соединения с БД
        /// </summary>
        public PMEContext(PmConfig? configurator = null, IServiceProvider? serviceProvider = null)
        {
            _configurator = configurator;
            _serviceProvider = serviceProvider;
            // Инициализация БД теперь выполняется через PmMigrationInitializer
        }

        public PMEContext(DbContextOptions options) : base(options) { }

        /// <summary>
        /// Конфигурироание контекста
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

            if (_configurator is not null)
                switch (_configurator.DataProvider)
                {
                    case DataProvider.PG:
                        optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_configurator.ConnectionString ?? "Host=99.99.999.99;Port=5432;Database=demobase;Username=migration;Password=migration");
                        break;
                    case DataProvider.SQLite:
                        optionsBuilder.UseLazyLoadingProxies().UseSqlite(_configurator.ConnectionString ?? "Filename=dataset.db");
                        break;
                    case DataProvider.InMemory:
                        optionsBuilder.UseLazyLoadingProxies().UseSqlite("DataSource=file::memory:?cache=shared");
                        break;
                }
            else
                optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_configurator?.ConnectionString ?? "Host=99.99.999.99;Port=5432;Database=demobase;Username=migration;Password=migration");
        }

        /// <summary>
        /// Конфигурация модели БД. Может быть переопределена в дочерних классах для добавления дополнительных сущностей
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Специальная конфигурация для SQLite - автоматически применяется ко всем сущностям
            if (_configurator?.DataProvider == DataProvider.SQLite || _configurator?.DataProvider == DataProvider.InMemory)
            {
                // Для SQLite нужно использовать INTEGER вместо BIGINT для первичных ключей с автоинкрементом
                // SQLite поддерживает AUTOINCREMENT только для INTEGER PRIMARY KEY
                // Автоматически применяем ко всем сущностям с первичными ключами типа long
                ConfigureSqlitePrimaryKeys(modelBuilder);
            }
            
            // Регистрация дополнительных конфигураций из дочерних библиотек
            if (_serviceProvider is not null)
            {
                var configurators = _serviceProvider.GetServices<IDbContextConfigurator>();
                foreach (var configurator in configurators.OrderBy(c => c.Priority))
                {
                    configurator.ConfigureModel(modelBuilder);
                }
            }
        }

        /// <summary>
        /// Автоматически конфигурирует все первичные ключи типа long для SQLite
        /// Применяется ко всем сущностям во всех наследуемых контекстах
        /// </summary>
        /// <param name="modelBuilder"></param>
        private void ConfigureSqlitePrimaryKeys(ModelBuilder modelBuilder)
        {
            // Получаем все типы сущностей из модели (включая сущности из дочерних контекстов)
            var entityTypes = modelBuilder.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                // Получаем первичный ключ сущности
                var primaryKey = entityType.FindPrimaryKey();
                
                if (primaryKey != null && primaryKey.Properties.Count == 1)
                {
                    // Обрабатываем только простые первичные ключи (не составные)
                    var keyProperty = primaryKey.Properties[0];
                    
                    // Если свойство имеет тип long
                    if (keyProperty.ClrType == typeof(long) || keyProperty.ClrType == typeof(long?))
                    {
                        // Проверяем, что это автоинкрементируемое свойство
                        // (ValueGenerated.OnAdd или свойство с именем Id/заканчивающееся на Id)
                        var isAutoIncrement = keyProperty.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd ||
                                              keyProperty.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                                              (keyProperty.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && 
                                               keyProperty.ValueGenerated != Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);

                        if (isAutoIncrement)
                        {
                            try
                            {
                                // Применяем конфигурацию INTEGER для SQLite
                                var entityBuilder = modelBuilder.Entity(entityType.ClrType);
                                entityBuilder.Property(keyProperty.Name)
                                    .HasColumnType("INTEGER")
                                    .ValueGeneratedOnAdd();
                            }
                            catch
                            {
                                // Игнорируем ошибки, если сущность уже была сконфигурирована
                                // (например, в дочернем контексте или через IDbContextConfigurator)
                            }
                        }
                    }
                }
            }
        }
    }
}