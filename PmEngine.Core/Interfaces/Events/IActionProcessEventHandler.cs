namespace PmEngine.Core.Interfaces.Events
{
    /// <summary>
    /// Событие выполнения действия ПЕРЕД действием в случае полного действия пользователя (Нажал на кнопку)
    /// </summary>
    public interface IActionProcessBeforeEventHandler : IEventHandler
    {
        /// <summary>
        /// Отлов события
        /// </summary>
        /// <param name="userSession">ID пользователя</param>
        /// <param name="action">Действие</param>
        /// <returns></returns>
        public Task Handle(IUserSession userSession, IActionWrapper action);
    }

    /// <summary>
    /// Событие выполнения действия ПОСЛЕ действия в случае полного действия пользователя (Нажал на кнопку)
    /// </summary>
    public interface IActionProcessAfterEventHandler : IEventHandler
    {
        /// <summary>
        /// Отлов события
        /// </summary>
        /// <param name="userSession">ID пользователя</param>
        /// <param name="action">Действие</param>
        /// <returns></returns>
        public Task Handle(IUserSession userSession, IActionWrapper action);
    }

    /// <summary>
    /// Событие выполнения действия ПОСЛЕ действия в случае полного действия пользователя (Нажал на кнопку)
    /// </summary>
    public interface IActionProcessAfterOutputEventHandler : IEventHandler
    {
        /// <summary>
        /// Отлов события
        /// </summary>
        /// <param name="userSession">ID пользователя</param>
        /// <param name="action">Действие</param>
        /// <returns></returns>
        public Task Handle(IUserSession userSession, IActionWrapper action);
    }
}