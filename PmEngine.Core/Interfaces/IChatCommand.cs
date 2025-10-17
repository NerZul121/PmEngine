namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Интерфейс исполняемой пользователем команды
    /// </summary>
    public interface IChatCommand
    {
        /// <summary>
        /// Имя команды
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Описание команды
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Тип юзера, которому доступна команда
        /// </summary>
        public int UserType { get; }

        /// <summary>
        /// Действие выполнения команды
        /// </summary>
        /// <param name="text"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> DoCommand(string text, IUserSession user);
    }
}