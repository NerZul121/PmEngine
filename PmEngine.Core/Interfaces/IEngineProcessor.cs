using PmEngine.Core.Interfaces.Events;

namespace PmEngine.Core.Interfaces
{
    public interface IEngineProcessor
    {
        public Task ActionProcess(IActionWrapper actionWrapper, IUserSession session, IActionArguments arguments);
        public Task<INextActionsMarkup?> MakeAction(IActionWrapper actionWrapper, IUserSession session, IActionArguments arguments);
        public Task MakeEvent<T>(Func<T, Task> evnt) where T : IEventHandler;
    }
}
