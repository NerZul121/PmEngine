using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Entities;

namespace PmEngine.Core.Interfaces
{
    public interface IUserSession : IDisposable
    {
        public long Id { get { return CachedData.Id; } }
        public UserEntity Data { get; }
        public UserEntity CachedData { get; }
        public UserEntity Reload(BaseContext context);
        public ActionWrapper? InputAction { get; set; }
        public INextActionsMarkup? NextActions { get; set; }
        public ActionWrapper? CurrentAction { get; set; }
        public T GetOutput<T>() where T : IOutputManager;
        public string? OutputContent { get; set; }
        public IEnumerable<object>? Media { get; set; }
        public IServiceProvider Services { get; protected set; }
        public IOutputManager GetOutputOrCreate(Type outputType);
        public Task MarkOnline();
        public T? GetLocal<T>(string name);
        public void SetLocal(string name, object? value);
        public DateTime SessionCreateTime { get; }
        public IOutputManager Output { get; }
        public void AddToOutput(string text);
        public ILogger Logger { get; set; }
        public Arguments LocalStore { get; }

        public async Task ActionProcess(ActionWrapper action)
        {
            var processor = Services.GetRequiredService<IEngineProcessor>();
            await processor.ActionProcess(action, this);
        }

        public async Task<INextActionsMarkup?> MakeAction(ActionWrapper action)
        {
            var processor = Services.GetRequiredService<IEngineProcessor>();
            return await processor.MakeAction(action, this);
        }
    }
}