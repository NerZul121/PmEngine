using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;
using PmEngine.Core.Interfaces.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Extensions;
using PmEngine.Core.Entities;
using System.Text.Json;
using PmEngine.Core.SessionElements;

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

        public EngineProcessor(IServiceProvider services, ILogger<EngineProcessor> logger)
        {
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Выполнение пользователем выбранного действия c последствиями и учетом следующих действий. Т.е. считаем, что пользователь нажал на кнопку и ему должны вернуться списком его следующие действия.
        /// </summary>
        /// <param name="action">Действие</param>
        /// <param name="arguments"></param>
        /// <param name="userSession"></param>
        public async Task ActionProcess(IActionWrapper action, IUserSession userSession, IActionArguments arguments)
        {
            try
            {
                if (action is null)
                    return;

                userSession.NextActions = null;
                userSession.CurrentAction = action;
                userSession.InputAction = null;

                await MakeEvent<IActionProcessBeforeEventHandler>((handler) => handler.Handle(userSession, action));

                INextActionsMarkup? result = null;

                IAction? iaction = null;

                if (action.ActionType is null)
                    result = action.NextActions;

                else if (action.ActionType.GetInterface("IAction") == null)
                    throw new Exception(action.ActionType + " не реализует интерфейс IAction.");

                else
                {
                    await MakeEvent<IMakeActionBeforeEventHandler>((handler) => handler.Handle(userSession, action));

                    iaction = (IAction?)Activator.CreateInstance(action.ActionType);

                    if (iaction is null)
                        throw new Exception("Не удалось создать экшн " + action.ActionType);

                    result = await iaction.DoAction(action, userSession, action.Arguments);
                }

                if (result is not null && result.GetNextActions().Any())
                    userSession.NextActions = result;

                var output = userSession.OutputContent + action.ActionText;

                if (string.IsNullOrEmpty(output))
                    output = null;

                await MakeEvent<IActionProcessAfterEventHandler>((handler) => handler.Handle(userSession, action));

                int msgId = -1;
                if (!String.IsNullOrEmpty(output) || userSession.Media is not null && userSession.Media.Any())
                    msgId = await userSession.GetOutput().ShowContent(output, result?.NumeredDuplicates(), userSession.Media, result?.Arguments);

                arguments.Set("messageId", msgId);

                if (iaction is not null)
                    await iaction.AfterAction(action, userSession, action.Arguments);

                await MakeEvent<IMakeActionAfterEventHandler>((handler) => handler.Handle(userSession, action));

                await userSession.MarkOnline();
                await MakeEvent<IActionProcessAfterOutputEventHandler>((handler) => handler.Handle(userSession, action));

                var engine = _services.GetRequiredService<IEngineConfigurator>();
                if (engine.Properties.EnableStateless)
                {
                    await _services.InContext(async (context) =>
                    {
                        var userData = await userSession.Reload(context);
                        userData.SessionData = userSession.NextActions is null ? null : JsonSerializer.Serialize(new SessionData(userSession.NextActions));
                        await context.SaveChangesAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка исполнения {action}: {ex}", userSession.CachedData);
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
        public async Task<INextActionsMarkup?> MakeAction(IActionWrapper action, IUserSession user, IActionArguments? arguments = null)
        {
            _logger.LogInformation($"User ({user}): MakeAction {action.DisplayName} ({action.ActionType})");

            if (action.ActionType is null)
                return action.NextActions;

            if (action.ActionType.GetInterface("IAction") == null)
                throw new Exception(action.ActionType + " не реализует интерфейс IAction.");

            await MakeEvent<IMakeActionBeforeEventHandler>((handler) => handler.Handle(user, action));

            var act = (IAction?)Activator.CreateInstance(action.ActionType);

            if (act is null)
                throw new Exception("Не удалось создать экшн " + action.ActionType);

            var result = await act.DoAction(action, user, action.Arguments);

            await act.AfterAction(action, user, action.Arguments);

            await MakeEvent<IMakeActionAfterEventHandler>((handler) => handler.Handle(user, action));

            return result;
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
    }
}