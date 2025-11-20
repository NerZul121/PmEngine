using Microsoft.Extensions.Logging;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Daemons
{
    public abstract class BaseDaemon : IDaemon
    {
        protected IServiceProvider _services;
        protected ILogger _logger;
        public int DelayInSec { get; set; } = 1;
        public bool IsWork { get; set; } = true;
        public CancellationToken CancellationToken { get; set; }
        public string ProcessId { get; set; } = Guid.NewGuid().ToString();

        public BaseDaemon(IServiceProvider services, ILogger logger)
        {
            _services = services;
            _logger = logger;
        }

        /// <summary>
        /// ID процесса
        /// </summary>

        /// <summary>
        /// Запуск демона
        /// </summary>
        public async void Start()
        {
            _logger.LogInformation($"Launch daemon {GetType().FullName}");

            IsWork = true;
            CancellationToken = new CancellationToken();

            while (IsWork)
            {
                try
                {
                    try
                    {
                        await Work().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{GetType().FullName}: {ex}");
                    }

                    await Task.Delay(DelayInSec * 1000, CancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                        break;

                    _logger.LogError($"{ex}");
                }
            }
        }

        /// <summary>
        /// Основная логика демона
        /// </summary>
        /// <returns></returns>
        public abstract Task Work();

        public void Stop()
        {
            IsWork = false;
            CancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation($"Stopping daemon {GetType().FullName}");
        }
    }
}