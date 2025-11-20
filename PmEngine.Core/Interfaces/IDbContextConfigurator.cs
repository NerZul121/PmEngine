using Microsoft.EntityFrameworkCore;

namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для конфигурации контекста БД из дочерних библиотек.
    /// Позволяет добавлять дополнительные сущности и конфигурации в единый контекст БД.
    /// </summary>
    public interface IDbContextConfigurator
    {
        /// <summary>
        /// Приоритет конфигурации. 
        /// Чем ниже число - тем раньше (по сравнению с другими) произойдет конфигурация.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Конфигурация модели БД. Здесь можно добавить дополнительные сущности и их конфигурации.
        /// </summary>
        /// <param name="modelBuilder">ModelBuilder для конфигурации</param>
        void ConfigureModel(ModelBuilder modelBuilder);
    }
}



