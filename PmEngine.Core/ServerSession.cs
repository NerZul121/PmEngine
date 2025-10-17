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
    public class ServerSession 
    {
        private ILogger _logger;
        private IServiceProvider _services;
        private PmEngine _engine;

        public ServerSession(IServiceProvider services, ILogger logger, PmEngine engine)
        {
            _logger = logger;
            _services = services;
            _engine = engine;
        }

        /// <summary>
        /// Активные сессии пользователей
        /// </summary>
        public ConcurrentDictionary<long, IUserSession> UserSessions { get; private set; } = new();

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
        public virtual async Task<IUserSession> GetUserSession(long userId, Action<IUserSession>? init = null, Type? outputType = null, bool forceInit = true)
        {
            var session = UserSessions.ContainsKey(userId) ? UserSessions[userId] : null;

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

                if (outputType is not null)
                    session.GetOutputOrCreate(outputType);

                if (init != null)
                    init(session);

                if (forceInit)
                    if (!_engine.Properties.EnableStateless || String.IsNullOrEmpty(session.CachedData.SessionData))
                        await _services.GetRequiredService<IEngineProcessor>().ActionProcess(session.CurrentAction, session);
            }
            else if (outputType is not null)
                session.GetOutputOrCreate(outputType);

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