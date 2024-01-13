using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Extensions
{
    /// <summary>
    /// Расширение для DbContext, позволяющее использовать context.Set(Type type), где Type - это тип сущности.<br/>
    /// Это продвинутый функционал, который не рекомендуется к исползованию, если нет четкого понимания как он работает и зачем он нужен.
    /// </summary>
    public static class ContextExtansion
    {
        public static async Task InContext(this IServiceProvider services, Func<BaseContext, Task> func)
        {
            await services.GetRequiredService<IContextHelper>().InContext(func);
        }

        public static async Task InContext<T>(this IServiceProvider services, Func<T, Task> func) where T : IDataContext
        {
            await services.GetRequiredService<IContextHelper>().InContext<T>(func);
        }

        public static async Task InContext(this IServiceProvider services, Type contextType, Func<BaseContext, Task> func)
        {
            await services.GetRequiredService<IContextHelper>().InContext(contextType, func);
        }

        public static async Task<T> InContext<T>(this IServiceProvider services, Func<BaseContext, Task<T>> func)
        {
            return await services.GetRequiredService<IContextHelper>().InContext(func);
        }
    }
}