using Microsoft.Extensions.DependencyInjection;

namespace PmEngine.Core.Interfaces
{
    public interface IEngineConfigurator
    {
        public EngineProperties Properties { get; }
        public IServiceProvider ServiceProvider { get; }
        public bool Configurated { get; }
        public Task Configure(IServiceProvider services);
        public IServiceCollection Services { get; internal set; }
    }
}