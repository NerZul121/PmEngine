using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Daemons
{
    /// <summary>
    /// Фоновый процесс обработки сессий пользователей.
    /// </summary>
    public class UserDaemon : BaseDaemon
    {
        public UserDaemon(ILogger<UserDaemon> logger, IServiceProvider services) : base(services, logger)
        {
        }

        /// <summary>
        /// Запуск процесса.
        /// </summary>
        public override Task Work()
        {
            var users = _services.GetRequiredService<ServerSession>().GetAllSessions().ToArray();
            var tasks = new List<Task>();

            foreach (var user in users)
            {
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await Process(user).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                    }
                }));
            }

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Процесс обработки пользователя.
        /// </summary>
        /// <param name="userSession"></param>
        private async Task Process(UserSession userSession)
        {
            if (!await IsOnline(userSession).ConfigureAwait(false))
                return;
        }

        /// <summary>
        /// Делает проверку на онлайн пользователя. Если он должен быть не в сети - удаляет его сессию и возвращает false. Иначе возвращает true.
        /// </summary>
        /// <param name="userSession">Сессия пользователя</param>
        /// <returns></returns>
        public async Task<bool> IsOnline(UserSession userSession)
        {
            if (userSession is null)
                return false;

            if ((DateTime.Now - userSession.SessionCreateTime).TotalSeconds < 5)
                return true;

            if (userSession.CachedData.LastOnlineDate.AddMinutes(_services.GetRequiredService<PmConfig>().SessionLifeTime) > DateTime.Now)
                return true;

            await _services.GetRequiredService<ServerSession>().RemoveUserSession(userSession).ConfigureAwait(false);

            return false;
        }
    }
}