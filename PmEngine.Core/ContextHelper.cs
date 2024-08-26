using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    public class ContextHelper : IContextHelper
    {
        private IServiceProvider _services;

        public ContextHelper(IServiceProvider serviceProvider)
        {
            _services = serviceProvider;
        }

        public async Task InContext<T>(Func<T, Task> action) where T : IDataContext
        {
            try
            {
                using var scope = _services.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<T>();
                await action(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task InContext(Func<BaseContext, Task> action)
        {
            try
            {
                using var scope = _services.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<IDataContext>() as BaseContext;
                await action(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task InContext(Type contextType, Func<BaseContext, Task> action)
        {
            try
            {
                using var scope = _services.CreateScope();
                using var context = (BaseContext)scope.ServiceProvider.GetServices<IDataContext>().First(c => c.GetType() == contextType);
                await action(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<T> InContext<T>(Type contextType, Func<BaseContext, Task<T>> action)
        {
            try
            {
                using var scope = _services.CreateScope();
                using var context = (BaseContext)scope.ServiceProvider.GetServices<IDataContext>().First(c => c.GetType() == contextType);
                return await action(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<T> InContext<T>(Func<BaseContext, Task<T>> action)
        {
            try
            {
                using var scope = _services.CreateScope();
                using var context = scope.ServiceProvider.GetServices<IDataContext>().First(c => c.GetType() == typeof(BaseContext)) as BaseContext;
                return await action(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}