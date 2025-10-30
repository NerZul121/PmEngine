using PmEngine.Core.Interfaces;
using PmEngine.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace PmEngine.Core.Daemons
{
    /// <summary>
    /// Фоновый процесс обработки сессий пользователей.
    /// </summary>
    public class UserDaemon : BaseDaemon
    {
        public UserDaemon(ILogger logger, IServiceProvider services) : base(services, logger)
        {
        }

        /// <summary>
        /// Запуск процесса.
        /// </summary>
        public override Task Work()
        {
            var users = _services.GetRequiredService<ServerSession>().GetAllSessions().Where(v => v.CachedData.UserType != (int)UserType.Techincal && v.CachedData.UserType != (int)UserType.Banned).ToList();

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

            return Task.CompletedTask;
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

            if ((DateTime.Now - userSession.SessionCreateTime).TotalSeconds < 5)
                return true;

            if (userSession.CachedData.LastOnlineDate.AddMinutes(_services.GetRequiredService<PmEngine>().Properties.SessionLifeTime) > DateTime.Now)
                return true;

            await _services.GetRequiredService<ServerSession>().RemoveUserSession(userSession);

            return false;
        }
    }
}
