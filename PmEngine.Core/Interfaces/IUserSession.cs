﻿using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Entities;

namespace PmEngine.Core.Interfaces
{
    public interface IUserSession : IDisposable
    {
        public long Id { get { return CachedData.Id; } }
        public UserEntity Data { get; }
        public UserEntity CachedData { get; }
        public Task<UserEntity> Reload(BaseContext context);
        public IActionWrapper? InputAction { get; set; }
        public INextActionsMarkup? NextActions { get; set; }
        public IActionWrapper? CurrentAction { get; set; }
        public T GetOutput<T>() where T : IOutputManager;
        public IOutputManager GetOutput();
        public string? OutputContent { get; set; }
        public IEnumerable<object>? Media { get; set; }
        public IServiceScope Scope { get; protected set; }
        public IServiceProvider Services { get; protected set; }
        public void SetDefaultOutput<T>() where T : IOutputManager;
        public Task MarkOnline();
        public T? GetLocal<T>(string name);
        public void SetLocal(string name, object? value);
        public DateTime SessionCreateTime { get; }
        public IOutputManager Output { get; }
        public void AddToOutput(string text);

        public async Task ActionProcess(IActionWrapper action)
        {
            var processor = Services.GetRequiredService<IEngineProcessor>();
            await processor.ActionProcess(action, this, action.Arguments);
        }

        public async Task<INextActionsMarkup?> MakeAction(IActionWrapper action)
        {
            var processor = Services.GetRequiredService<IEngineProcessor>();
            return await processor.MakeAction(action, this, action.Arguments);
        }
    }
}