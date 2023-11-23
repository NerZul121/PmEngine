using PmEngine.Core.Enums;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    /// <summary>
    /// Настройки бота
    /// </summary>
    public class EngineProperties
    {
        /// <summary>
        /// Сколько минут человек считается онлайн с последнего своего действия
        /// </summary>
        public int SessionLifeTime { get; set; } = 120;

        /// <summary>
        /// Начальное действие при инициализации сессии пользователя
        /// </summary>
        public Type? InitializationAction { get; set; }

        /// <summary>
        /// Агрументы для начального действия
        /// </summary>
        public IActionArguments StartArguments { get; set; } = new ActionArguments();

        /// <summary>
        /// Используемый DataProvider
        /// </summary>
        public DataProvider DataProvider { get; set; } = (DataProvider)Convert.ToInt32(Environment.GetEnvironmentVariable("PROVIDER_TYPE") ?? "1");

        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        public string? ConnectionString { get; set; } = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        /// <summary>
        /// Использование поведение сохранения даты в формате UTC в PG
        /// </summary>
        public bool EnableLegacyTimestampBehavior { get; set; } = Convert.ToBoolean(Environment.GetEnvironmentVariable("EnableLegacyTimestampBehavior") ?? "true");

        /// <summary>
        /// Независимость от сессий
        /// </summary>
        public bool EnableStateless { get; set; } = true;
    }
}