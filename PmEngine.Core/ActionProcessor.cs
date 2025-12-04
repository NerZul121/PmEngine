using PmEngine.Core.Interfaces;
using PmEngine.Core.Interfaces.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core
{
    /// <summary>
    /// Основной процесс. <br/>
    /// Тут производятся основные функции для работы с действиями пользователей
    /// </summary>
    public class ActionProcessor : IActionProcessor
    {
        private ILogger<ActionProcessor> _logger;
        private IServiceProvider _services;
        private PmConfig _configuration;
        private Dictionary<Type, IAction> _actions;

        public ActionProcessor(IServiceProvider services, ILogger<ActionProcessor> logger, PmConfig config)
        {
            _logger = logger;
            _services = services;
            _configuration = config;
            _actions = _services.GetServices<IAction>().ToDictionary(a => a.GetType(), a => a);
        }

        /// <summary>
        /// Выполнение пользователем выбранного действия c последствиями и учетом следующих действий. Т.е. считаем, что пользователь нажал на кнопку и ему должны вернуться списком его следующие действия.
        /// </summary>
        /// <param name="action">Действие</param>
        /// <param name="userSession"></param>
        public async Task ActionProcess(ActionWrapper action, UserSession userSession)
        {
            try
            {
                userSession.Logger.LogInformation($"{userSession}: {action.DisplayName} ({action.ActionType} {action.ActionTypeName})");

                userSession.SetLocal("LastNextActions", userSession.NextActions);
                userSession.NextActions = null;
                userSession.CurrentAction = action;
                userSession.InputAction = null;

                await MakeEvent<IActionProcessBeforeEventHandler>(async (handler) => await handler.Handle(userSession, action).ConfigureAwait(false)).ConfigureAwait(false);
                INextActionsMarkup? result = null;
                IAction? iaction = null;
                
                // Если ActionType не установлен, пытаемся найти его по ActionTypeName
                if (action.ActionType is null)
                {
                    _logger.LogInformation($"ActionType is null, trying to resolve by ActionTypeName: {action.ActionTypeName}");
                    var at = GetActionType(action.ActionTypeName);

                    if (at is null)
                    {
                        // Тип не найден, используем NextActions из action
                        _logger.LogWarning($"ActionType not found for ActionTypeName: {action.ActionTypeName}, using action.NextActions");
                        result = action.NextActions;
                    }
                    else
                    {
                        // Тип найден, устанавливаем его и продолжаем выполнение
                        _logger.LogInformation($"ActionType resolved: {at.FullName}");
                        action.ActionType = at;
                    }
                }
                
                // Выполняем действие, если ActionType установлен
                if (action.ActionType is not null)
                {
                    if (action.ActionType.GetInterface("IAction") == null)
                        throw new Exception(action.ActionType + " not implement IAction.");
                    
                    if (!_actions.TryGetValue(action.ActionType, out iaction))
                        throw new Exception("Указанный Action не зарегистрирован в качестве сервиса");

                    if (iaction is null)
                        throw new Exception("Не удалось создать экшн " + action.ActionType);

                    _logger.LogInformation($"Executing action: {action.ActionType.FullName}");
                    result = await iaction.DoAction(action, userSession).ConfigureAwait(false);
                    _logger.LogInformation($"Action executed, result: {(result != null ? "has result" : "null")}");
                }
                else
                {
                    _logger.LogWarning($"ActionType is still null after resolution attempt, action will not be executed");
                }

                if (result is not null && result.GetNextActions().Any())
                    userSession.NextActions = result;

                await MakeEvent<IActionProcessAfterEventHandler>(async (handler) => await handler.Handle(userSession, action).ConfigureAwait(false)).ConfigureAwait(false);

                var output = userSession.OutputContent + action.ActionText;

                if (string.IsNullOrEmpty(output))
                    output = null;

                int msgId = -1;
                if (!String.IsNullOrEmpty(output) || userSession.Media is not null && userSession.Media.Any())
                    msgId = await userSession.Output.ShowContent(output, result?.NumeredDuplicates(), userSession.Media, result?.Arguments).ConfigureAwait(false);

                action.Arguments.Set("messageId", msgId);

                if (iaction is not null)
                    await iaction.AfterAction(action, userSession).ConfigureAwait(false);

                await userSession.MarkOnline().ConfigureAwait(false);
                await MakeEvent<IActionProcessAfterOutputEventHandler>(async (handler) => await handler.Handle(userSession, action).ConfigureAwait(false)).ConfigureAwait(false);

                if (_configuration.EnableStateless)
                    await userSession.SaveState().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var cachedData = userSession.CachedData;
                var args = new Arguments(new() { { "exception", ex }, { "action", action } });
                _logger.LogError($"{userSession.Id}: Ошибка исполнения {action}: {ex}", cachedData);
                userSession.OutputContent = "";
                userSession.Media = null;

                if (action.DisplayName == "ExceptionAction")
                    _logger.LogCritical($"{userSession.Id}: Ошибка исполнения {action}: {ex}", cachedData);
                else
                    await ActionProcess(new ActionWrapper("ExceptionAction", _configuration.ExceptionAction, args), userSession).ConfigureAwait(false);
            }
            finally
            {
                userSession.OutputContent = "";
                userSession.Media = null;
            }
        }

        /// <summary>
        /// Выполнение единичного действия.
        /// Вызывает событие IMakeActionEventHandler.
        /// </summary>
        /// <param name="action">Экшн</param>
        /// <returns>Результат выполнения</returns>
        /// <exception cref="Exception">Ошибки при инициализации/выполнени экшена</exception>
        public async Task<INextActionsMarkup?> MakeAction(ActionWrapper action, UserSession user)
        {
            _logger.LogInformation($"User ({user}): MakeAction {action.DisplayName} ({action.ActionType})");
            INextActionsMarkup? result = null;

            await MakeEvent<IMakeActionBeforeEventHandler>(async (handler) => await handler.Handle(user, action).ConfigureAwait(false)).ConfigureAwait(false);

            if (action.ActionType is null)
                return action.NextActions;

            if (action.ActionType.GetInterface("IAction") == null)
                throw new Exception(action.ActionType + " не реализует интерфейс IAction.");

            var act = (IAction?)Activator.CreateInstance(action.ActionType);

            if (act is null)
                throw new Exception("Не удалось создать экшн " + action.ActionType);

            result = await act.DoAction(action, user).ConfigureAwait(false);
            await act.AfterAction(action, user).ConfigureAwait(false);

            await MakeEvent<IMakeActionAfterEventHandler>(async (handler) => await handler.Handle(user, action).ConfigureAwait(false)).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Получение типа экшена
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public Type? GetActionType(string? actionName)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                _logger.LogWarning("GetActionType called with null or empty actionName");
                return null;
            }

            _logger.LogInformation($"GetActionType: searching for action with name: {actionName}");
            
            var allActions = _services.GetServices<IAction>().ToList();
            _logger.LogInformation($"GetActionType: found {allActions.Count} registered IAction services");
            
            var action = allActions.FirstOrDefault(a => a.GetType().FullName == actionName)?.GetType();
            
            if (action is null)
            {
                // Попробуем найти по имени без полного пути
                action = allActions.FirstOrDefault(a => a.GetType().Name == actionName)?.GetType();
                if (action is not null)
                    _logger.LogInformation($"GetActionType: found by Name (not FullName): {action.FullName}");
            }
            
            if (action is null)
            {
                _logger.LogWarning($"GetActionType: action not found for name: {actionName}. Available actions: {string.Join(", ", allActions.Select(a => a.GetType().FullName))}");
            }
            else
            {
                _logger.LogInformation($"GetActionType: found action type: {action.FullName}");
            }

            return action;
        }

        /// <summary>
        /// Выполнить все подписки на данное событие
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evnt"></param>
        /// <returns></returns>
        public async Task MakeEvent<T>(Func<T, Task> evnt) where T : IEventHandler
        {
            _logger.LogInformation($"Make event {typeof(T)}");
            var handlers = _services.GetServices<IEventHandler>().Where(s => s is T).Select(s => (T)s).ToArray();

            foreach (var handler in handlers)
            {
                try
                {
                    _logger.LogInformation($"Handle event {typeof(T)} -- {handler.GetType()}");
                    await evnt(handler).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error handling {t} -- {type}: {ex}", typeof(T), handler.GetType(), ex);
                }
            }
        }

        public void MakeEventSync<T>(Action<T> evnt) where T : IEventHandler
        {
            _logger.LogInformation($"Make event {typeof(T)}");
            var handlers = _services.GetServices<IEventHandler>().Where(s => s is T).Select(s => (T)s).ToArray();

            foreach (var handler in handlers)
            {
                try
                {
                    _logger.LogInformation($"Handle event {typeof(T)} -- {handler.GetType()}");
                    evnt(handler);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error handling {t} -- {type}: {ex}", typeof(T), handler.GetType(), ex);
                }
            }
        }
    }
}