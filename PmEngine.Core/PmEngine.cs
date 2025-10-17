using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;
using PmEngine.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Daemons;

namespace PmEngine.Core
{
    /// <summary>
    /// Класс движка. <br/>
    /// Тут происходит конфигурация модулей, их добавление и управление контекстами для работы с бд.
    /// </summary>
    public class PmEngine
    {
        public PmEngine(IServiceProvider services, EngineProperties properties)
        {
            _serviceProvider = services;
            Properties = properties;
        }

        /// <summary>
        /// Конфигурация движка
        /// </summary>
        public EngineProperties Properties { get; private set; }

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

        private ILogger<PmEngine> _logger { get { if (_loggercreated is null) _loggercreated = GetLogger<PmEngine>(); return _loggercreated; } }
        private ILogger<PmEngine>? _loggercreated;

        /// <summary>
        /// Конфигурирование движка. <br/>
        /// На этом моменте выполняется конфигурация всех зарегистрированных модулей движка, запуск менеджеров и инициализация приложения.
        /// </summary>
        public async Task Configure(IServiceProvider services)
        {
            try
            {
                if (Properties.EnableLegacyTimestampBehavior)
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                if (Properties.DataProvider != DataProvider.PG)
                {
                    try
                    {
                        using var context = new BaseContext(this);
                        context.Database.EnsureCreated();
                    }
                    catch (Exception ex) { }
                }

                LogConfig();

                if (Properties.DataProvider == DataProvider.PG)
                {
                    using var contextScope = _serviceProvider.CreateScope();
                    var contexts = contextScope.ServiceProvider.GetServices<IDataContext>().Where(c => c.GetType().ToString() != typeof(BaseContext).ToString());
                    await Migrate(typeof(BaseContext));

                    foreach (var context in contexts)
                        try
                        {
                            await Migrate(context.GetType());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Migration exception of context {context.GetType().FullName}: {ex}");
                            throw;
                        }
                }

                foreach (var content in _serviceProvider.GetServices<IContentRegistrator>().OrderBy(p => p.Priority))
                    await content.Registrate();

                _serviceProvider.GetRequiredService<DaemonManager>().Configure();
                _serviceProvider.GetRequiredService<CommandManager>().Configure();
            }
            catch (Exception ex)
            {
                _logger.LogError("Configuration error: " + ex.ToString());
                throw;
            }
        }

        private void LogConfig()
        {
            var props = typeof(EngineProperties).GetProperties();

            foreach (var prop in props)
                _logger.LogInformation($"{(prop.Name == "ConnectionString" ? $"{prop.Name}: ***HASVALUE***" : $"{prop.Name}: {prop.GetValue(Properties)}")}");
        }

        /// <summary>
        /// Автоматическая миграция для БД
        /// </summary>
        /// <param name="contextType"></param>
        private async Task Migrate(Type contextType)
        {
            try
            {
                await ServiceProvider.GetRequiredService<IContextHelper>().InContext(contextType, async (context) =>
                {
                    _logger.LogInformation($"Checking migrations for context {contextType} ...");

                    if (context.Database.GetPendingMigrations().Any())
                    {
                        _logger.LogInformation($"Context {contextType} has migrations. Migrationg...");
                        await context.Database.MigrateAsync();
                        _logger.LogInformation($"Context {contextType} migrated.");
                    }
                    else
                        _logger.LogInformation($"No pending migrations fo context {contextType}");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Migration error {contextType}: {ex}");
            }
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