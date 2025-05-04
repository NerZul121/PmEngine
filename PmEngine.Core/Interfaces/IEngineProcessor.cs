using PmEngine.Core.Interfaces.Events;

namespace PmEngine.Core.Interfaces
{
    public interface IEngineProcessor
    {
        public Task ActionProcess(ActionWrapper actionWrapper, IUserSession session);
        public Task<INextActionsMarkup?> MakeAction(ActionWrapper actionWrapper, IUserSession session);
        public Task MakeEvent<T>(Func<T, Task> evnt) where T : IEventHandler;
        public void MakeEventSync<T>(Action<T> evnt) where T : IEventHandler;
    }
}
