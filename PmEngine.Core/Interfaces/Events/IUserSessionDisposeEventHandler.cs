using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Interfaces.Events
{
    /// <summary>
    /// Событие очистки сессии пользователя.
    /// </summary>
    /// Когда пользователь становится оффлайн (проходит определенное кол-во времени с последнего действия) сессия очищается, предвадительно вызывая это событие.
    public interface UserSessionDisposeEventHandler : IEventHandler
    {
        /// <summary>
        /// Обработка события
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public Task Handle(UserSession session);
    }
}