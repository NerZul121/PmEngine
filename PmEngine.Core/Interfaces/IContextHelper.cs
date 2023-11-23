﻿using PmEngine.Core.BaseClasses;

namespace PmEngine.Core.Interfaces
{
    public interface IContextHelper
    {
        public Task InContext<T>(Func<T, Task> action) where T : IDataContext;
        public Task InContext(Func<BaseContext, Task> action);
        public Task InContext(Type contextType, Func<BaseContext, Task> action);
    }
}