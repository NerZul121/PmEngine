using PmEngine.Core.Interfaces.Events;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Interfaces
{
    public interface IActionProcessor
    {
        public Task ActionProcess(ActionWrapper actionWrapper, UserSession session);
        public Task<INextActionsMarkup?> MakeAction(ActionWrapper actionWrapper, UserSession session);
        public Task MakeEvent<T>(Func<T, Task> evnt) where T : IEventHandler;
        public void MakeEventSync<T>(Action<T> evnt) where T : IEventHandler;
    }
}
