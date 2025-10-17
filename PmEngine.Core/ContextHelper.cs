using PmEngine.Core.BaseClasses;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    public class ContextHelper : IContextHelper
    {
        private PmEngine _engine;

        public ContextHelper(PmEngine engine)
        {
            _engine = engine;
        }

        public async Task InContext(Func<BaseContext, Task> action)
        {
            try
            {
                using var context = new BaseContext(_engine);
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
                using var context = new BaseContext(_engine);
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
                using var context = new BaseContext(_engine);
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
                using var context = new BaseContext(_engine);
                return await action(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public void InContextSync(Action<BaseContext> act)
        {
            using var context = new BaseContext(_engine);
            act(context);
        }

        public T InContextSync<T>(Func<BaseContext, T> func)
        {
            using var context = new BaseContext(_engine);
            return func(context);
        }
    }
}