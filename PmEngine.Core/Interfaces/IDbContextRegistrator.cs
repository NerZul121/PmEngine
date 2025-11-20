using Microsoft.EntityFrameworkCore;
using PmEngine.Core;

namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для регистрации контекстов БД из дочерних библиотек.
    /// Позволяет автоматически выполнять миграции для всех зарегистрированных контекстов при инициализации приложения.
    /// </summary>
    public interface IDbContextRegistrator
    {
        /// <summary>
        /// Создает экземпляр контекста БД для инициализации и миграций.
        /// </summary>
        /// <param name="config">Конфигурация PmEngine</param>
        /// <param name="serviceProvider">Провайдер сервисов</param>
        /// <returns>Экземпляр DbContext</returns>
        DbContext CreateContext(PmConfig config, IServiceProvider serviceProvider);

        /// <summary>
        /// Типы контекстов, от которых зависит этот контекст.
        /// Контексты будут инициализированы в порядке зависимостей:
        /// сначала контексты без зависимостей, затем по зависимостям.
        /// PMEContext всегда инициализируется первым и не требует указания в зависимостях.
        /// </summary>
        /// <example>
        /// Если ChatBotContext зависит от TelegramContext, верните: new[] { typeof(TelegramContext) }
        /// </example>
        Type[]? DependsOn { get; }
    }
}

