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
        private List<IDaemon> _daemons = new();
        private readonly IServiceProvider _serviceProvider;
        public static List<string> DaemonsToLoad { get; private set; } = new();

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
        /// </summary>eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
        /// <param name="services">Сервис провайдер</param>
        public void Configure(IServiceProvider services)
        {
            var daemonsScope = services.CreateScope();
            var daemonsServices = daemonsScope.ServiceProvider;
            var owner = daemonsServices.GetRequiredService<IUserScopeData>();
            owner.Owner = new EmptyUserSession();
            owner.Services = daemonsServices;

            var daemons = daemonsServices.GetServices<IDaemon>().ToList();

            foreach(var rd in DaemonsToLoad)
            {
                var d = LoadDaemon(rd);
                if(d != null)
                    daemons.Add(d);
            }

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

        public IDaemon? GetDaemon(string fullname)
        {
            return Daemons.FirstOrDefault(d => d.GetType().FullName == fullname);
        }

        public IDaemon? LoadDaemon<T>() where T : IDaemon
        {
            return LoadDaemon(typeof(T).FullName);
        }

        public IDaemon? LoadDaemon(string fullname)
        {
            return Assembler.Get<IDaemon>(fullname, new object?[] { _serviceProvider, _logger });
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

        public void ReloadDaemon<T>() where T : IDaemon
        {
            var daemon = LoadDaemon<T>();
            var exist = GetDaemon<T>();

            if (exist != null)
            {
                exist.Stop();
                _daemons.Remove(exist);
            }

            daemon.Start();
            _daemons.Add(daemon);
        }

        public void ReloadDaemon(string fullname)
        {
            var daemon = LoadDaemon(fullname);
            var exist = GetDaemon(fullname);

            if (exist != null)
            {
                exist.Stop();
                _daemons.Remove(exist);
            }

            daemon.Start();
            _daemons.Add(daemon);
        }
    }
}