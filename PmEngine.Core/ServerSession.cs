using PmEngine.Core.SessionElements;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using PmEngine.Core.Entities;
using PmEngine.Core.Interfaces.Events;
using PmEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace PmEngine.Core
{
    /// <summary>
    /// Текущая сессия сервера
    /// </summary>
    public class ServerSession : IServerSession
    {
        private ILogger _logger;
        private IServiceProvider _services;

        public ServerSession(IServiceProvider services, ILogger logger)
        {
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Активные сессии пользователей
        /// </summary>
        public static ConcurrentDictionary<long, IUserSession> UserSessions { get; private set; } = new();

        /// <summary>
        /// Попытка взять сессию пользователя без её создания
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Активная сессия пользователя или null</returns>
        /// <exception cref="Exception">Если пользователь не найден - вернется ошибка</exception>
        public virtual IUserSession? TryGetUserSession(long userId)
        {
            var session = UserSessions.ContainsKey(userId) ? UserSessions[userId] : null;

            return session;
        }

        /// <summary>
        /// Получение/создание сессии пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns></returns>
        public virtual async Task<IUserSession> GetUserSession(long userId, Action<IUserSession>? init = null)
        {
            var session = UserSessions.ContainsKey(userId) ? UserSessions[userId] : null;
            var engine = _services.GetRequiredService<IEngineConfigurator>();

            if (session is null)
            {
                await _services.GetRequiredService<IContextHelper>().InContext(async (context) =>
                {
                    var usr = await context.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
                    if (usr is null)
                        throw new Exception("Пользователь не найден. ID: " + userId);

                    session = new UserSession(_services, usr);
                });

                UserSessions[userId] = session;

                if (init != null)
                    init(session);

                if (!engine.Properties.EnableStateless || String.IsNullOrEmpty(session.CachedData.SessionData))
                    await _services.GetRequiredService<IEngineProcessor>().ActionProcess(session.CurrentAction, session);
            }

            return session;
        }

        /// <summary>
        /// Удаляет сессию пользователя
        /// </summary>
        /// <param name="userSession">ID пользователя</param>
        public virtual async Task RemoveUserSession(IUserSession userSession)
        {
            var us = UserSessions[userSession.CachedData.Id];
            await _services.GetRequiredService<IEngineProcessor>().MakeEvent<IUserSessionDisposeEventHandler>(async (handler) => await handler.Handle(us));
            UserSessions.Remove(userSession.CachedData.Id, out _);
        }

        public virtual IEnumerable<IUserSession> GetAllSessions()
        {
            return UserSessions.Values;
        }
    }
}