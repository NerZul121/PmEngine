using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmEngine.Core.Interfaces;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Daemons
{
    /// <summary>
    /// Менеджер фоновых процессов.<br/>
    /// Запускает фоновые процессы, реализующие интерфейс IDaemon и зарегистрированные в движке.
    /// </summary>
    public class DaemonManager : IManager
    {
        private readonly ILogger<DaemonManager> _logger;
        public DaemonManager(ILogger<DaemonManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Список всех фоновых процессов
        /// </summary>
        public static IEnumerable<IDaemon> Daemons { get; private set; } = Enumerable.Empty<IDaemon>();

        /// <summary>
        /// Запускаем зарегистрированные фоновые процессы
        /// </summary>
        /// <param name="services">Сервис провайдер</param>
        public void Configure(IServiceProvider services)
        {
            var daemonsScope = services.CreateScope();
            var daemonsServices = daemonsScope.ServiceProvider;
            var owner = daemonsServices.GetRequiredService<IUserScopeData>();
            owner.Owner = new EmptyUserSession();
            owner.Services = daemonsServices;

            var daemons = daemonsServices.GetServices<IDaemon>();
            var dt = new List<IDaemon>();
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
                    dt.Add(daemon);

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
        public static T GetDaemon<T>() where T : IDaemon
        {
            return (T)Daemons.First(d => d.GetType() == typeof(T));
        }
    }
}