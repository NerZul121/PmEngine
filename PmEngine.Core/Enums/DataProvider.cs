namespace PmEngine.Core.Enums
{
    /// <summary>
    /// Используемый провайдер данных
    /// </summary>
    public enum DataProvider
    {
        /// <summary>
        /// PostgreSQL
        /// </summary>
        PG,
        /// <summary>
        /// SQLite
        /// </summary>
        SQLite,
        /// <summary>
        /// SQLite, но в формате InMemory
        /// </summary>
        InMemory,
    }
}