using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Entities;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.SessionElements
{
    public class EmptyUserSession : IUserSession
    {
        public UserEntity Data => CachedData;

        public UserEntity CachedData { get { return _data; } }

        private UserEntity _data = new UserEntity() { Id = -99 };

        public ActionWrapper? InputAction { get; set; }
        public INextActionsMarkup? NextActions { get; set; }
        public ActionWrapper? CurrentAction { get; set; }
        public string? OutputContent { get; set; }
        public IEnumerable<object>? Media { get; set; }

        public DateTime SessionCreateTime { get; set; } = DateTime.Now;

        public IOutputManager Output { get; set; }

        ActionWrapper? IUserSession.InputAction { get; set; }
        INextActionsMarkup? IUserSession.NextActions { get; set; }
        ActionWrapper? IUserSession.CurrentAction { get; set; }
        string? IUserSession.OutputContent { get; set; }
        IEnumerable<object>? IUserSession.Media { get; set; }
        IServiceScope IUserSession.Scope { get; set; }
        IServiceProvider IUserSession.Services { get; set; }

        DateTime IUserSession.SessionCreateTime { get; } = DateTime.Now;

        IOutputManager IUserSession.Output { get; }

        public void AddToOutput(string text)
        {

        }

        public void Dispose()
        {

        }

        public T? GetLocal<T>(string name)
        {
            throw new NotImplementedException();
        }

        public T GetOutput<T>() where T : IOutputManager
        {
            throw new NotImplementedException();
        }

        public IOutputManager GetOutput()
        {
            throw new NotImplementedException();
        }

        public Task MarkOnline()
        {
            return Task.CompletedTask;
        }

        public Task<UserEntity> Reload(BaseContext context)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultOutput<T>() where T : IOutputManager
        {
        }

        public void SetLocal(string name, object? value)
        {
        }
    }
}