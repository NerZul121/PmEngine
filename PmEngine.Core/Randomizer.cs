namespace PmEngine.Core
{
    /// <summary>
    /// Рандомайзер
    /// </summary>
    public static class Randomizer
    {
        private static int seed = Environment.TickCount;

        private static ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        /// <summary>
        /// Получить случайное число
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Next(int min, int max)
        {
            if (max < min)
                return _random.Value.Next(max, min);
            else
                return _random.Value.Next(min, max);
        }
    }
}