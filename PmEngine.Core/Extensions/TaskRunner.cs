namespace PmEngine.Core.Extensions
{
    /// <summary>
    /// Запуск задачи в отдельном потоке, которая не будет ожидаться
    /// </summary>
    public static class TaskRunner
    {
        /// <summary>
        /// Запуск задачи в отдельном потоке, которая не будет ожидаться
        /// </summary>
        /// <param name="action"></param>
        public static async void Run(Action action)
        {
            action();
        }

        /// <summary>
        /// Запуск задачи в отдельном потоке, которая не будет ожидаться
        /// </summary>
        /// <param name="action"></param>
        public static async void Run(Func<Task> action)
        {
            action();
        }
    }
}