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
        private readonly ConcurrentDictionary<long, SemaphoreSlim> _sessionCreationLocks = new();

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
            if (UserSessions.TryGetValue(userId, out var existingSession))
            {
                if (outputType is not null)
                    existingSession.GetOutputOrCreate(outputType);

                return existingSession;
            }

            var semaphore = _sessionCreationLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                if (UserSessions.TryGetValue(userId, out existingSession))
                {
                    if (outputType is not null)
                        existingSession.GetOutputOrCreate(outputType);
                    return existingSession;
                }

                IUserSession? newSession = null;
                await _services.GetRequiredService<IContextHelper>().InContext(async (context) =>
                {
                    var usr = await context.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == userId);
                    if (usr is null)
                        throw new Exception("Пользователь не найден. ID: " + userId);

                    newSession = new UserSession(_services, usr);
                });

                if (newSession is not null)
                {
                    UserSessions.TryAdd(userId, newSession);

                    if (outputType is not null)
                        newSession.GetOutputOrCreate(outputType);

                    if (init != null)
                        init(newSession);

                    if (forceInit)
                        if (!_engine.Properties.EnableStateless || String.IsNullOrEmpty(newSession.CachedData.SessionData))
                            await _services.GetRequiredService<IEngineProcessor>().ActionProcess(newSession.CurrentAction, newSession);

                    return newSession;
                }

                throw new Exception("Не удалось создать сессию пользователя. ID: " + userId);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Удаляет сессию пользователя
        /// </summary>
        /// <param name="userSession">ID пользователя</param>
        public virtual async Task RemoveUserSession(IUserSession userSession)
        {
            await _services.GetRequiredService<IEngineProcessor>().MakeEvent<IUserSessionDisposeEventHandler>(async (handler) => await handler.Handle(userSession));
            UserSessions.Remove(userSession.CachedData.Id, out _);
        }

        public virtual IEnumerable<IUserSession> GetAllSessions()
        {
            return UserSessions.Values;
        }
    }
}