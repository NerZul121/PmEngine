using PmEngine.Core.Interfaces;
using PmEngine.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace PmEngine.Core.Daemons
{
    /// <summary>
    /// Фоновый процесс обработки сессий пользователей.
    /// </summary>
    public class UserDaemon : IDaemon
    {
        private ILogger _logger;
        private IServiceProvider _services;

        public UserDaemon(ILogger logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Запуск процесса.
        /// </summary>
        public void Start()
        {
            while (true)
            {
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        var users = _services.GetRequiredService<IServerSession>().GetAllSessions().Where(v => v.CachedData.UserType != (int)UserType.Techincal && v.CachedData.UserType != (int)UserType.Banned).ToList();

                        foreach (var user in users)
                        {
                            Task.Factory.StartNew(async () =>
                            {
                                try
                                {
                                    await Process(user);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex.ToString());
                                }
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError("USER DAEMON: " + ex.ToString());
                }

                Task.Delay(2500).Wait();
            }
        }

        /// <summary>
        /// Процесс обработки пользователя.
        /// </summary>
        /// <param name="userSession"></param>
        private async Task Process(IUserSession userSession)
        {
            if (userSession.CachedData.UserType == (int)UserType.Banned)
                return;

            if (!await IsOnline(userSession))
                return;
        }

        /// <summary>
        /// Делает проверку на онлайн пользователя. Если он должен быть не в сети - удаляет его сессию и возвращает false. Иначе возвращает true.
        /// </summary>
        /// <param name="userSession">Сессия пользователя</param>
        /// <returns></returns>
        public async Task<bool> IsOnline(IUserSession userSession)
        {
            if (userSession is null)
                return false;

            if (userSession.CachedData.LastOnlineDate.AddMinutes(_services.GetRequiredService<IEngineConfigurator>().Properties.SessionLifeTime) > DateTime.Now)
                return true;            

            await _services.GetRequiredService<IServerSession>().RemoveUserSession(userSession);

            return false;
        }
    }
}
