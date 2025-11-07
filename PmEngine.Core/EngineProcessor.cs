using PmEngine.Core.Interfaces;
using PmEngine.Core.Interfaces.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Actions;

namespace PmEngine.Core
{
    /// <summary>
    /// Основной процесс. <br/>
    /// Тут производятся основные функции для работы с действиями пользователей
    /// </summary>
    public class EngineProcessor : IEngineProcessor
    {
        private ILogger<EngineProcessor> _logger;
        private IServiceProvider _services;
        private PmEngine _config;

        public EngineProcessor(IServiceProvider services, ILogger<EngineProcessor> logger, PmEngine config)
        {
            _logger = logger;
            _services = services;
            _config = config;
        }

        /// <summary>
        /// Выполнение пользователем выбранного действия c последствиями и учетом следующих действий. Т.е. считаем, что пользователь нажал на кнопку и ему должны вернуться списком его следующие действия.
        /// </summary>
        /// <param name="action">Действие</param>
        /// <param name="userSession"></param>
        public async Task ActionProcess(ActionWrapper action, IUserSession userSession)
        {
            try
            {
                userSession.Logger.LogInformation($"{userSession}: {action.DisplayName} ({action.ActionType} {action.ActionTypeName})");

                userSession.SetLocal("LastNextActions", userSession.NextActions);
                userSession.NextActions = null;
                userSession.CurrentAction = action;
                userSession.InputAction = null;

                await MakeEvent<IActionProcessBeforeEventHandler>((handler) => handler.Handle(userSession, action));

                INextActionsMarkup? result = null;

                IAction? iaction = null;

                await MakeEvent<IMakeActionBeforeEventHandler>((handler) => handler.Handle(userSession, action));


                if (action.ActionType is null)
                {
                    var at = GetActionType(action.ActionTypeName);

                    if (at is null)
                        result = action.NextActions;
                    else
                        action.ActionType = at;
                }

                if (action.ActionType.GetInterface("IAction") == null)
                    throw new Exception(action.ActionType + " не реализует интерфейс IAction.");
                else
                {
                    iaction = (IAction?)Activator.CreateInstance(action.ActionType);

                    if (iaction is null)
                        throw new Exception("Не удалось создать экшн " + action.ActionType);

                    result = await iaction.DoAction(action, userSession);
                }

                if (result is not null && result.GetNextActions().Any())
                    userSession.NextActions = result;

                await MakeEvent<IActionProcessAfterEventHandler>((handler) => handler.Handle(userSession, action));

                var output = userSession.OutputContent + action.ActionText;

                if (string.IsNullOrEmpty(output))
                    output = null;

                int msgId = -1;
                if (!String.IsNullOrEmpty(output) || userSession.Media is not null && userSession.Media.Any())
                    msgId = await userSession.Output.ShowContent(output, result?.NumeredDuplicates(), userSession.Media, result?.Arguments);

                action.Arguments.Set("messageId", msgId);

                if (iaction is not null)
                    await iaction.AfterAction(action, userSession);

                await MakeEvent<IMakeActionAfterEventHandler>((handler) => handler.Handle(userSession, action));

                await userSession.MarkOnline();
                await MakeEvent<IActionProcessAfterOutputEventHandler>((handler) => handler.Handle(userSession, action));
            }
            catch (Exception ex)
            {
                var cachedData = userSession.CachedData;
                var args = new Arguments(new() { { "exception", ex }, { "action", action } });
                _logger.LogError($"{userSession.Id}: Ошибка исполнения {action}: {ex}", cachedData);
                userSession.OutputContent = "";
                userSession.Media = null;

                if (action.ActionType == (_config.Properties.ExceptionAction ?? typeof(ExceptionAction)))
                    _logger.LogCritical($"{userSession.Id}: Ошибка исполнения {action}: {ex}", cachedData);
                else
                    await ActionProcess(new ActionWrapper("ExceptionAction", _config.Properties.ExceptionAction ?? typeof(ExceptionAction), args), userSession);
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
        public async Task<INextActionsMarkup?> MakeAction(ActionWrapper action, IUserSession user)
        {
            _logger.LogInformation($"User ({user}): MakeAction {action.DisplayName} ({action.ActionType})");
            INextActionsMarkup? result = null;

            await MakeEvent<IMakeActionBeforeEventHandler>((handler) => handler.Handle(user, action));

            if (action.ActionType is null)
                return action.NextActions;

            if (action.ActionType.GetInterface("IAction") == null)
                throw new Exception(action.ActionType + " не реализует интерфейс IAction.");

            var act = (IAction?)Activator.CreateInstance(action.ActionType);

            if (act is null)
                throw new Exception("Не удалось создать экшн " + action.ActionType);

            result = await act.DoAction(action, user);
            await act.AfterAction(action, user);

            await MakeEvent<IMakeActionAfterEventHandler>((handler) => handler.Handle(user, action));

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
                return null;

            Type? action = null;

            if (action is null)
                action = _services.GetServices<IAction>().FirstOrDefault(a => a.GetType().FullName == actionName)?.GetType();

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
            var handlers = _services.GetServices<IEventHandler>().Where(s => s is T).Select(s => (T)s);

            foreach (var handler in handlers)
                await evnt(handler);
        }

        public void MakeEventSync<T>(Action<T> evnt) where T : IEventHandler
        {
            var handlers = _services.GetServices<IEventHandler>().Where(s => s is T).Select(s => (T)s);

            foreach (var handler in handlers)
                evnt(handler);
        }
    }
}