namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Интерфейс фонового процесса. Он будет запущен автоматически при конфигурации движка.<br/>
	/// Если вам необходима постоянная работа в фоне - то реализуйте цикл while(true) внутри и отлов исключений.
    /// </summary>
    public interface IDaemon
	{
		/// <summary>
		/// Запуск фонового процесса
		/// </summary>
		public void Start();
	}
}