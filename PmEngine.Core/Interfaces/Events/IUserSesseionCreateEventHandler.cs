using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Interfaces.Events
{
    /// <summary>
    /// Событие инициализации сессии 
    /// </summary>
    public interface IUserSesseionInitializeEventHandler : IEventHandler
    {
        /// <summary>
        /// Обработка инициализации сессии пользователя
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public Task Handle(UserSession session);
    }
}