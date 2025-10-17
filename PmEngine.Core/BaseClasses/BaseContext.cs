using Microsoft.EntityFrameworkCore;
using PmEngine.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Attributes;
using PmEngine.Core.Enums;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PmEngine.Core.BaseClasses
{
    /// <summary>
    /// Базовый контекст соединения с БД. <br/>
    /// Для работы с контекстом необходимо использовать context.Set
    /// </summary>
    [Priority(int.MaxValue)]
    public class BaseContext : DbContext, IDataContext
    {
        protected PmEngine _configurator;

        /// <summary>
        /// Контекст соединения с БД
        /// </summary>
        public BaseContext(PmEngine? configurator = null)
        {
            if (configurator is not null)
            {
                _configurator = configurator;

                if (configurator.Properties.DataProvider == DataProvider.SQLite)
                    Database.EnsureCreated();

                if (configurator.Properties.DataProvider == DataProvider.InMemory)
                {
                    Database.EnsureDeleted();
                    Database.EnsureCreated();
                }
            }
        }

        public BaseContext(DbContextOptions options) : base(options) { }

        /// <summary>
        /// Создание модели контекста. Подтягиваются все сущности, зарегистрированные в системе, реализующие интерфейс IDataEntity
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (_configurator is null)
                foreach (var type in GetType().Assembly.GetTypes().Where(t => t.GetInterface("IDataEntity") != null && !t.IsAbstract))
                    modelBuilder.Entity(type);
            else
                foreach (Type type in _configurator.ServiceProvider.GetServices<IDataEntity>().Select(s => s.GetType()))
                    modelBuilder.Entity(type);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Конфигурироание контекста
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

            if (_configurator is not null)
                switch (_configurator.Properties.DataProvider)
                {
                    case DataProvider.PG:
                        optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_configurator.Properties.ConnectionString ?? "Host=99.99.999.99;Port=5432;Database=demobase;Username=migration;Password=migration");
                        break;
                    case DataProvider.SQLite:
                        optionsBuilder.UseLazyLoadingProxies().UseSqlite(_configurator.Properties.ConnectionString ?? "Filename=dataset.db");
                        break;
                    case DataProvider.InMemory:
                        optionsBuilder.UseLazyLoadingProxies().UseSqlite("DataSource=file::memory:?cache=shared");
                        break;
                }
            else
                optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_configurator?.Properties.ConnectionString ?? "Host=99.99.999.99;Port=5432;Database=demobase;Username=migration;Password=migration");
        }
    }
}