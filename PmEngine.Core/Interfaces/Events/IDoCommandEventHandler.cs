namespace PmEngine.Core.Interfaces.Events
{
    /// <summary>
    /// События выполнения команды пользователем
    /// </summary>
    public interface IDoCommandEventHandler : IEventHandler
    {
        /// <summary>
        /// Обработка события
        /// </summary>
        /// <param name="text">Текст команды</param>
        /// <param name="user">ID пользователя</param>
        public Task Handle(string text, IUserSession user);
    }
}