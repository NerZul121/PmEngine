using PmEngine.Core.SessionElements;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using PmEngine.Core.Entities;
using PmEngine.Core.Interfaces.Events;
using PmEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using PmEngine.Core.BaseClasses;

namespace PmEngine.Core
{
    /// <summary>
    /// Текущая сессия сервера
    /// </summary>
    public class ServerSession
    {
        private ILogger _logger;
        private IServiceProvider _services;
        private PmConfig _config;
        public PmConfig Config { get { return _config; } }

        public ServerSession(IServiceProvider services, ILogger<ServerSession> logger, PmConfig config)
        {
            _logger = logger;
            _services = services;
            _config = config;
        }

        /// <summary>
        /// Активные сессии пользователей
        /// </summary>
        public ConcurrentDictionary<long, UserSession> UserSessions { get; private set; } = new();

        /// <summary>
        /// Попытка взять сессию пользователя без её создания
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Активная сессия пользователя или null</returns>
        /// <exception cref="Exception">Если пользователь не найден - вернется ошибка</exception>
        public virtual UserSession? TryGetUserSession(long userId)
        {
            var session = UserSessions.ContainsKey(userId) ? UserSessions[userId] : null;
            return session;
        }

        /// <summary>
        /// Получение/создание сессии пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns></returns>
        public virtual async Task<UserSession> GetUserSession(long userId, Action<UserSession>? init = null, Type? outputType = null)
        {
            var session = UserSessions.ContainsKey(userId) ? UserSessions[userId] : null;

            if (session is null)
            {
                using var context = new PMEContext(_config, _services);
                var usr = await context.Set<UserEntity>().AsNoTracking().Include(u => u.Permissions).FirstOrDefaultAsync(p => p.Id == userId).ConfigureAwait(false);
                if (usr is null)
                    throw new Exception("User with ID " + userId + " not found.");

                session = new UserSession(_services, usr);

                if (outputType is not null)
                    session.DefaultOutput = session.GetOutputOrCreate(outputType);

                if (init != null)
                    init(session);

                bool stateRestored = false;
                if (_config.EnableStateless && !string.IsNullOrEmpty(session.CachedData.SessionData))
                {
                    var sessionData = JsonSerializer.Deserialize<SessionData>(session.CachedData.SessionData);
                    if (sessionData is not null)
                    {
                        session.NextActions = sessionData.NextActions(_services);
                        session.CurrentAction = sessionData.CurrentAction?.Wrap(_services);
                        session.InputAction = sessionData.InputAction?.Wrap(_services);
                        stateRestored = true;
                        _logger.LogInformation($"User {userId}: State restored from SessionData. NextActions: {session.NextActions != null}, CurrentAction: {session.CurrentAction?.DisplayName}, InputAction: {session.InputAction?.DisplayName}");
                    }
                }
                else
                    session.CurrentAction = new ActionWrapper("Initialization", _config.InitializationAction, _config.StartArguments);

                // Сохраняем состояние NextActions до вызова события, чтобы проверить, не затерли ли их
                var nextActionsBeforeEvent = stateRestored ? session.NextActions : null;

                await _services.GetRequiredService<IActionProcessor>().MakeEvent<IUserSesseionInitializeEventHandler>(async (handler) => await handler.Handle(session).ConfigureAwait(false)).ConfigureAwait(false);

                // Проверяем, не были ли затерты NextActions обработчиком события
                if (stateRestored && nextActionsBeforeEvent != null && session.NextActions == null)
                {
                    _logger.LogWarning($"User {userId}: NextActions were cleared by IUserSesseionInitializeEventHandler handler! Restoring them back.");
                    session.NextActions = nextActionsBeforeEvent;
                }

                UserSessions[userId] = session;

                // Не вызываем ActionProcess если состояние было восстановлено из БД в stateless режиме
                // Это предотвращает затирание NextActions, которые были восстановлены
                if (_config.InitializeWithAction && !stateRestored)
                    await _services.GetRequiredService<IActionProcessor>().ActionProcess(session.CurrentAction, session).ConfigureAwait(false);
            }
            else if (outputType is not null)
                session.DefaultOutput = session.GetOutputOrCreate(outputType);

            return session;
        }

        /// <summary>
        /// Удаляет сессию пользователя
        /// </summary>
        /// <param name="userSession">ID пользователя</param>
        public virtual async Task RemoveUserSession(UserSession userSession)
        {
            await _services.GetRequiredService<IActionProcessor>().MakeEvent<UserSessionDisposeEventHandler>(async (handler) => await handler.Handle(userSession).ConfigureAwait(false)).ConfigureAwait(false);
            UserSessions.Remove(userSession.CachedData.Id, out _);
        }

        public virtual IEnumerable<UserSession> GetAllSessions()
        {
            return UserSessions.Values;
        }
    }
}