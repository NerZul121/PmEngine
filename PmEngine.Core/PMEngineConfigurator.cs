using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;
using PmEngine.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;

namespace PmEngine.Core
{
    /// <summary>
    /// Класс движка. <br/>
    /// Тут происходит конфигурация модулей, их добавление и управление контекстами для работы с бд.
    /// </summary>
    public class PMEngineConfigurator : IEngineConfigurator
    {
        /// <summary>
        /// Конфигурация движка
        /// </summary>
        public EngineProperties Properties { get; } = new EngineProperties();

        /// <summary>
        /// Сервис провайдер для получения сервисов
        /// </summary>
        private IServiceProvider? _serviceProvider;

        /// <summary>
        /// Список зарегистрированных сервисов
        /// </summary>
        public IServiceCollection Services { get; set; } = new ServiceCollection();

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

        private ILogger<PMEngineConfigurator> _logger { get { if (_loggercreated is null) _loggercreated = GetLogger<PMEngineConfigurator>(); return _loggercreated; } }
        private ILogger<PMEngineConfigurator>? _loggercreated;

        /// <summary>
        /// Конфигурирование движка. <br/>
        /// На этом моменте выполняется конфигурация всех зарегистрированных модулей движка, запуск менеджеров и инициализация приложения.
        /// </summary>
        public async Task Configure(IServiceProvider services)
        {
            try
            {
                _serviceProvider = services;

                if (services is null)
                    _serviceProvider = Services.BuildServiceProvider();

                LogConfig();

                if (Properties.EnableLegacyTimestampBehavior)
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                if (Properties.DataProvider == DataProvider.PG)
                {
                    using var contextScope = _serviceProvider.CreateScope();
                    var contexts = contextScope.ServiceProvider.GetServices<IDataContext>().Where(c => c.GetType().ToString() != typeof(BaseContext).ToString());
                    await Migrate(typeof(BaseContext));

                    foreach (var context in contexts)
                        await Migrate(context.GetType());
                }

                foreach (var content in _serviceProvider.GetServices<IContentRegistrator>().OrderBy(p => p.Priority))
                    await content.Registrate();

                var managersServices = _serviceProvider.GetServices<IManager>();
                foreach (var m in managersServices)
                    m.Configure(_serviceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError("Configuration error: " + ex.ToString());
                _serviceProvider = null;
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