using Microsoft.Extensions.DependencyInjection;

namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Отвечает за попадание модуля и его частей в движок.
    /// </summary>
    public interface IModuleRegistrator
    {
        /// <summary>
        /// Регистрация модуля движка
        /// </summary>
        /// <param name="services"></param>
        public void Registrate(IServiceCollection services);
    }
}