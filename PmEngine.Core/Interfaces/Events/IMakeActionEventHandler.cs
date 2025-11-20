using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Interfaces.Events
{
    /// <summary>
    /// Событие начала выполнения ActionWrapper через MakeAction
    /// </summary>
    public interface IMakeActionBeforeEventHandler : IEventHandler
    {
        /// <summary>
        /// Отлов события
        /// </summary>
        /// <param name="user">ID пользователя</param>
        /// <param name="action">действие</param>
        /// <returns></returns>
        public Task Handle(UserSession user, ActionWrapper action);
    }

    /// <summary>
    /// Событие окончания выполнения ActionWrapper через MakeAction
    /// </summary>
    public interface IMakeActionAfterEventHandler : IEventHandler
    {
        /// <summary>
        /// Отлов события
        /// </summary>
        /// <param name="user">ID пользователя</param>
        /// <param name="action">Действие</param>
        /// <returns></returns>
        public Task Handle(UserSession user, ActionWrapper action);
    }
}