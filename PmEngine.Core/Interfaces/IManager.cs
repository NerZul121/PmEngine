namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Менеджер, который сконфигурируется после конфигурации движка. Он может управлять чем-нибудь.<br/>
    /// Например менеджер фоновых процессов управляет фоновыми процессами.
    /// </summary>
    public interface IManager
    {
        /// <summary>
        /// Конфигурация менеджера
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Configure(IServiceProvider serviceProvider);
    }
}