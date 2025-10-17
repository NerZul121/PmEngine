using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;
using PmEngine.Core.Interfaces.Events;
using Microsoft.Extensions.Logging;

namespace PmEngine.Core
{
    /// <summary>
    /// Менеджер команд. <br/>
    /// Хранит в себе зарегистрированные команды и предоставляет функционал их выполнения с проверкой/игнорированием прав.
    /// </summary>
    public class CommandManager
    {
        private ILogger _logger;
        private IServiceProvider _services;

        /// <summary>
        /// Manager of commands
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="services"></param>
        public CommandManager(ILogger logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;

        }
        /// <summary>
        /// Список зарегистрированых команд
        /// </summary>
        public static Dictionary<string, IChatCommand> Commands { get; } = new();

        /// <summary>
        /// Конфигурация команд
        /// </summary>
        /// <param name="services"></param>
        public void Configure()
        {
            var commands = _services.GetServices<IChatCommand>();

            _logger.LogInformation($"Finded {commands.Count()} commands: {String.Join("; ", commands.Select(c => c.Name))}");

            foreach (var command in commands)
            {
                _logger.LogInformation("Command registration: " + command.ToString());
                if (Commands.ContainsKey(command.Name))
                    continue;

                Commands.Add(command.Name, command);
            }
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="user">Пользователь</param>
        /// <param name="ignoreRights">Игнорировать права</param>
        /// <returns>Выполнена/не выполнена</returns>
        public async Task<bool> DoCommand(string text, IUserSession user, bool ignoreRights = false)
        {
            var output = user.Output;

            try
            {
                var commandFirst = text.Split(' ').First().ToLower().Trim('/');

                if (!Commands.ContainsKey(commandFirst) || (user.CachedData.UserType < Commands[commandFirst].UserType && !ignoreRights))
                {
                    await output.ShowContent("Команда не найдена.");
                    return false;
                }

                await _services.GetRequiredService<IEngineProcessor>().MakeEvent<IDoCommandEventHandler>(async (handler) => { await handler.Handle(text, user); });

                return await Commands[commandFirst].DoCommand(text, user);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"При выполнении команды произошла ошибка: {ex}", user);
                return false;
            }
        }
    }
}