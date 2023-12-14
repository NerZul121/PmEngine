using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Interfaces;
using System.Reflection;

namespace PmEngine.Core.Extensions
{
    /// <summary>
    /// Расширение для DbContext, позволяющее использовать context.Set(Type type), где Type - это тип сущности.<br/>
    /// Это продвинутый функционал, который не рекомендуется к исползованию, если нет четкого понимания как он работает и зачем он нужен.
    /// </summary>
    public static class ContextExtansion
    {
        /// <summary>
        /// Получить набор данных по типу, основываясь на базовом типе TBase
        /// </summary>
        /// <typeparam name="TBase">Базовый тип</typeparam>
        /// <param name="context">Контекст</param>
        /// <param name="type">Тип</param>
        /// <returns></returns>
        public static IQueryable<TBase> Set<TBase>(this DbContext context, Type type) where TBase : IDataEntity
        {
            MethodInfo method = typeof(DbContext).GetMethods().Single(mi => mi.Name == "Set" && !mi.GetParameters().Any());
            MethodInfo generic = method.MakeGenericMethod(type);
            return (IQueryable<TBase>?)generic.Invoke(context, null) ?? throw new Exception("Не удалось создать Set с указанными данными.");
        }

        public static async Task InContext(this IServiceProvider services, Func<BaseContext, Task> func)
        {
            await services.GetRequiredService<IContextHelper>().InContext(func);
        }

        public static async Task InContext<T>(this IServiceProvider services, Func<T, Task> func) where T: IDataContext
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