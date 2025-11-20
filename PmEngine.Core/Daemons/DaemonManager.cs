using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Daemons
{
    /// <summary>
    /// Менеджер фоновых процессов.<br/>
    /// Запускает фоновые процессы, реализующие интерфейс IDaemon и зарегистрированные в движке.
    /// </summary>
    public class DaemonManager
    {
        private readonly ILogger<DaemonManager> _logger;
        private List<IDaemon> _daemons = new();
        private readonly IServiceProvider _serviceProvider;

        public DaemonManager(ILogger<DaemonManager> logger, IServiceProvider services)
        {
            _logger = logger;
            _serviceProvider = services;
        }

        /// <summary>
        /// Список всех фоновых процессов
        /// </summary>
        public IEnumerable<IDaemon> Daemons { get { return _daemons; } }

        /// <summary>
        /// Запускаем зарегистрированные фоновые процессы
        /// </summary>
        public void Configure()
        {
            var daemonsScope = _serviceProvider.CreateScope();
            var daemonsServices = daemonsScope.ServiceProvider;

            var daemons = daemonsServices.GetServices<IDaemon>().ToList();

            var types = new List<Type>();

            _logger.LogInformation($"Finded {daemons.Count()} daemons: {string.Join(" ;", daemons)}");

            foreach (var daemon in daemons)
            {
                try
                {
                    _logger.LogInformation("Registration daemon: " + daemon.ToString());
                    if (types.Any(d => d == daemon.GetType()))
                        continue;

                    _logger.LogInformation("Launch daemon: " + daemon.ToString());
                    types.Add(daemon.GetType());
                    _daemons.Add(daemon);

                    Task.Factory.StartNew(daemon.Start);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed to start {ex}");
                }
            }
        }

        /// <summary>
        /// Получение конкретного фонового процесса
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetDaemon<T>() where T : IDaemon
        {
            return (T?)Daemons.FirstOrDefault(d => d.GetType() == typeof(T));
        }

        public void StopDaemon<T>() where T : IDaemon
        {
            var daemon = _daemons.FirstOrDefault(d => d.GetType() == typeof(T));
            if (daemon != null)
                daemon.Stop();
        }

        public void StartDaemon<T>() where T : IDaemon
        {
            var daemon = _daemons.FirstOrDefault(d => d.GetType() == typeof(T));

            if (daemon != null)
                daemon.Start();
        }

        public void RestartDaemon<T>() where T : IDaemon
        {
            var daemon = _daemons.FirstOrDefault(d => d.GetType() == typeof(T));

            if (daemon is null)
                return;

            daemon.Stop();
            Task.Delay(1000).Wait();
            daemon.Start();
        }
    }
}