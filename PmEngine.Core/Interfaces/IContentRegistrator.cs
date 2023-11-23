namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Предоставляет метод регистрации контента модуля. <br/>
    /// Выполняется в последнюю очередь после конфигурации движка.<br/>
    /// Нужен, например, для создания/обновления заготовленных данных в БД.<br/>
    /// </summary>
    public interface IContentRegistrator
    {
        /// <summary>
        /// Приоритет. <br/>
        /// Чем ниже число - тем раньше (по сравнению с другими) произойдет регистрация.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Процесс регистрации контента
        /// </summary>
        public Task Registrate();
    }
}