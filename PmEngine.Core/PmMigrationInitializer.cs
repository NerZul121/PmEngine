using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;
using PmEngine.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Daemons;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PmEngine.Core
{
    /// <summary>
    /// Класс движка. <br/>
    /// Тут происходит конфигурация модулей, их добавление и управление контекстами для работы с бд.
    /// </summary>
    public class PmMigrationInitializer
    {
        public PmMigrationInitializer(IServiceProvider services, PmConfig config)
        {
            _serviceProvider = services;
            _config = config;
        }

        /// <summary>
        /// Конфигурация движка
        /// </summary>
        private PmConfig _config;

        /// <summary>
        /// Сервис провайдер для получения сервисов
        /// </summary>
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Сервис провайдер для получения сервисов.
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider is null)
                    throw new Exception("Engine not configured.");

                return _serviceProvider;
            }
        }

        /// <summary>
        /// Сконфигурирован-ли движок
        /// </summary>
        public bool Configurated { get { return _serviceProvider is not null; } }

        private ILogger<PmMigrationInitializer> _logger { get { if (_loggercreated is null) _loggercreated = GetLogger<PmMigrationInitializer>(); return _loggercreated; } }
        private ILogger<PmMigrationInitializer>? _loggercreated;

        /// <summary>
        /// Конфигурирование движка. <br/>
        /// На этом моменте выполняется конфигурация всех зарегистрированных модулей движка, запуск менеджеров и инициализация приложения.
        /// </summary>
        public async Task Configure(IServiceProvider services)
        {
            try
            {
                LogConfig();

                if (_config.EnableLegacyTimestampBehavior)
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                // Инициализация всех контекстов
                var contextRegistrators = _serviceProvider.GetServices<IDbContextRegistrator>().ToList();

                // Список всех контекстов для инициализации
                var contextsToInitialize = new List<(Func<DbContext> Factory, string Name, IDbContextRegistrator? Registrator, Type ContextType)>();

                // Добавляем базовый PMEContext (всегда первый)
                contextsToInitialize.Add(
                    (() => new PMEContext(_config, _serviceProvider), "PMEContext", null, typeof(PMEContext))
                );

                // Добавляем все зарегистрированные контексты
                foreach (var registrator in contextRegistrators)
                {
                    // Создаем временный контекст только для определения типа и имени
                    using var tempContext = registrator.CreateContext(_config, _serviceProvider);
                    var contextType = tempContext.GetType();
                    var contextName = contextType.Name; // Используем имя типа контекста
                    contextsToInitialize.Add(
                        (() => registrator.CreateContext(_config, _serviceProvider), contextName, registrator, contextType)
                    );
                }

                // Сортируем контексты по зависимостям (топологическая сортировка)
                var sortedContexts = SortContextsByDependencies(contextsToInitialize);

                // Инициализируем контексты в правильном порядке
                foreach (var (factory, name, registrator, _) in sortedContexts)
                {
                    await InitializeContextAsync(factory, name, registrator).ConfigureAwait(false);
                }

                // Регистрация контента
                foreach (var content in _serviceProvider.GetServices<IContentRegistrator>().OrderBy(p => p.Priority))
                    await content.Registrate().ConfigureAwait(false);

                _serviceProvider.GetRequiredService<DaemonManager>().Configure();
                _serviceProvider.GetRequiredService<CommandManager>().Configure();
            }
            catch (Exception ex)
            {
                _logger.LogError("Configuration error: " + ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Инициализация контекста БД с выполнением миграций
        /// </summary>
        /// <param name="contextFactory">Фабрика для создания контекста</param>
        /// <param name="contextName">Имя контекста для логирования</param>
        /// <param name="registrator">Регистратор контекста (для создания временного контекста при необходимости)</param>
        private async Task InitializeContextAsync(Func<DbContext> contextFactory, string contextName, IDbContextRegistrator? registrator = null)
        {
            try
            {
                using var context = contextFactory();

                if (_config.DataProvider == DataProvider.PG)
                {
                    // Для PostgreSQL используем миграции
                    var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                    if (pendingMigrations.Any())
                    {
                        _logger.LogInformation($"Applying {pendingMigrations.Count} pending migration(s) for {contextName}: {string.Join(", ", pendingMigrations)}");
                        await context.Database.MigrateAsync().ConfigureAwait(false);
                        _logger.LogInformation($"Migrations applied successfully for {contextName}");
                    }
                    else
                    {
                        _logger.LogInformation($"No pending migrations for {contextName}");
                    }
                }
                else if (_config.DataProvider == DataProvider.SQLite)
                {
                    // Для SQLite не используем миграции (они обычно созданы для PostgreSQL)
                    // Просто вызываем EnsureCreated для каждого контекста
                    // EnsureCreated создаст БД, если её нет, или добавит недостающие таблицы
                    bool dbExists = false;
                    try
                    {
                        dbExists = context.Database.CanConnect();
                    }
                    catch { }

                    if (!dbExists)
                    {
                        // БД не существует - создаем её со всеми таблицами из этого контекста
                        _logger.LogInformation($"Creating database for {contextName} using EnsureCreated()");
                        context.Database.EnsureCreated();
                        _logger.LogInformation($"Database created successfully for {contextName}");
                    }
                    else
                    {
                        // БД существует - EnsureCreated не создаст новые таблицы автоматически
                        // Нужно вручную проверить и создать недостающие таблицы
                        _logger.LogInformation($"Database exists for {contextName}, ensuring all tables are created");
                        await EnsureTablesFromContextAsync(context, contextName, registrator).ConfigureAwait(false);
                    }
                }
                else if (_config.DataProvider == DataProvider.InMemory)
                {
                    context.Database.EnsureCreated();
                    _logger.LogInformation($"InMemory database created for {contextName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing context {contextName}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Обеспечивает создание всех таблиц из модели контекста, даже если БД уже существует
        /// </summary>
        private async Task EnsureTablesFromContextAsync(DbContext context, string contextName, IDbContextRegistrator? registrator = null)
        {
            try
            {
                var entityTypes = context.Model.GetEntityTypes();
                var connection = context.Database.GetDbConnection();
                
                await connection.OpenAsync().ConfigureAwait(false);
                try
                {
                    var tablesToCreate = new List<string>();

                    foreach (var entityType in entityTypes)
                    {
                        var tableName = entityType.GetTableName();
                        if (string.IsNullOrEmpty(tableName))
                            continue;

                        var checkTableSql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = checkTableSql;
                            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (!reader.HasRows)
                                {
                                    tablesToCreate.Add(tableName);
                                }
                            }
                        }
                    }

                    if (tablesToCreate.Any())
                    {
                        _logger.LogInformation($"Creating {tablesToCreate.Count} missing table(s) for {contextName}: {string.Join(", ", tablesToCreate)}");
                        await CreateMissingTablesAsync(context, tablesToCreate, contextName, registrator).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not verify/create tables for {contextName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает недостающие таблицы через SQL, сгенерированный из модели
        /// </summary>
        private async Task CreateMissingTablesAsync(DbContext context, List<string> tableNames, string contextName, IDbContextRegistrator? registrator = null)
        {
            var tempDbFile = $"temp_{Guid.NewGuid()}.db";
            var tempConnectionString = $"Filename={tempDbFile}";
            DbContext? tempContext = null;

            try
            {
                // Создаем временный контекст
                if (registrator != null)
                {
                    var tempConfig = new PmConfig { DataProvider = _config.DataProvider, ConnectionString = tempConnectionString };
                    tempContext = registrator.CreateContext(tempConfig, _serviceProvider!);
                }
                else
                {
                    var contextType = context.GetType();
                    var constructor = contextType.GetConstructor(new[] { typeof(PmConfig), typeof(IServiceProvider) });
                    if (constructor != null)
                    {
                        var tempConfig = new PmConfig { DataProvider = _config.DataProvider, ConnectionString = tempConnectionString };
                        tempContext = constructor.Invoke(new object[] { tempConfig, _serviceProvider! }) as DbContext;
                    }
                }

                if (tempContext == null)
                    throw new Exception($"Could not create temporary context of type {context.GetType()}");

                // Создаем временную БД и получаем SQL
                tempContext.Database.EnsureCreated();
                var tempConnection = tempContext.Database.GetDbConnection();
                await tempConnection.OpenAsync().ConfigureAwait(false);

                var mainConnection = context.Database.GetDbConnection();
                var wasOpen = mainConnection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                    await mainConnection.OpenAsync().ConfigureAwait(false);

                try
                {
                    foreach (var tableName in tableNames)
                    {
                        // Получаем CREATE TABLE SQL из временной БД
                        string? createSql = null;
                        using (var cmd = tempConnection.CreateCommand())
                        {
                            cmd.CommandText = $"SELECT sql FROM sqlite_master WHERE type='table' AND name='{tableName}'";
                            using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                if (await reader.ReadAsync().ConfigureAwait(false))
                                    createSql = reader.GetString(0);
                            }
                        }

                        if (!string.IsNullOrEmpty(createSql))
                        {
                            // Применяем SQL к основной БД
                            using (var cmd = mainConnection.CreateCommand())
                            {
                                cmd.CommandText = createSql;
                                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                            }
                            _logger.LogInformation($"Created table '{tableName}' for {contextName}");
                        }
                    }
                }
                finally
                {
                    await tempConnection.CloseAsync().ConfigureAwait(false);
                    if (!wasOpen)
                        await mainConnection.CloseAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                try
                {
                    tempContext?.Database.EnsureDeleted();
                    if (System.IO.File.Exists(tempDbFile))
                        System.IO.File.Delete(tempDbFile);
                }
                catch { }
                tempContext?.Dispose();
            }
        }

        /// <summary>
        /// Сортирует контексты по зависимостям используя топологическую сортировку.
        /// PMEContext всегда первый, затем контексты без зависимостей, затем по зависимостям.
        /// </summary>
        private List<(Func<DbContext> Factory, string Name, IDbContextRegistrator? Registrator, Type ContextType)> 
            SortContextsByDependencies(List<(Func<DbContext> Factory, string Name, IDbContextRegistrator? Registrator, Type ContextType)> contexts)
        {
            var result = new List<(Func<DbContext> Factory, string Name, IDbContextRegistrator? Registrator, Type ContextType)>();
            var visited = new HashSet<Type>();
            var visiting = new HashSet<Type>();

            // PMEContext всегда первый
            var pmeContext = contexts.FirstOrDefault(c => c.ContextType == typeof(PMEContext));
            if (pmeContext.Factory != null)
            {
                result.Add(pmeContext);
                visited.Add(typeof(PMEContext));
            }

            // Функция для рекурсивного обхода зависимостей
            void Visit((Func<DbContext> Factory, string Name, IDbContextRegistrator? Registrator, Type ContextType) context)
            {
                if (visited.Contains(context.ContextType))
                    return;

                if (visiting.Contains(context.ContextType))
                {
                    _logger.LogWarning($"Circular dependency detected for context {context.Name}. Dependencies will be ignored.");
                    result.Add(context);
                    visited.Add(context.ContextType);
                    visiting.Remove(context.ContextType);
                    return;
                }

                visiting.Add(context.ContextType);

                // Обрабатываем зависимости
                if (context.Registrator?.DependsOn != null && context.Registrator.DependsOn.Any())
                {
                    foreach (var dependencyType in context.Registrator.DependsOn)
                    {
                        // PMEContext уже обработан, пропускаем
                        if (dependencyType == typeof(PMEContext))
                            continue;

                        var dependency = contexts.FirstOrDefault(c => c.ContextType == dependencyType);
                        if (dependency.Factory != null && !visited.Contains(dependencyType))
                        {
                            Visit(dependency);
                        }
                        else if (dependency.Factory == null)
                        {
                            _logger.LogWarning($"Dependency {dependencyType.Name} for context {context.Name} not found. It will be ignored.");
                        }
                    }
                }

                visiting.Remove(context.ContextType);
                if (!visited.Contains(context.ContextType))
                {
                    result.Add(context);
                    visited.Add(context.ContextType);
                }
            }

            // Обрабатываем все контексты (кроме PMEContext, который уже добавлен)
            foreach (var context in contexts)
            {
                if (context.ContextType != typeof(PMEContext))
                {
                    Visit(context);
                }
            }

            return result;
        }

        private void LogConfig()
        {
            var props = typeof(PmConfig).GetProperties();

            foreach (var prop in props)
                _logger.LogInformation($"{(prop.Name == "ConnectionString" ? $"{prop.Name}: ***HASVALUE***" : $"{prop.Name}: {prop.GetValue(_config)}")}");
        }

        /// <summary>
        /// Получение логера
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ILogger<T> GetLogger<T>()
        {
            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

            return loggerFactory.CreateLogger<T>();
        }
    }
}