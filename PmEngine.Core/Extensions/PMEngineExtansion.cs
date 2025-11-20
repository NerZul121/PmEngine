using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Daemons;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class PMEngineExtansion
    {
        /// <summary>
        /// Интеграция движка с билдерами
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPMEngine(this IServiceCollection services, Action<PmConfig>? config = null)
        {
            var cfg = new PmConfig();
            if (config is not null)
                config(cfg);

            services.AddSingleton(cfg);
            services.AddSingleton<PmMigrationInitializer>();
            services.AddSingleton<DaemonManager>();
            services.AddSingleton<CommandManager>();
            services.AddSingleton(typeof(IActionProcessor), typeof(ActionProcessor));
            services.AddSingleton<ServerSession>();
            services.AddSingleton<ILocalHelper, LocalHelper>();

            var baseModule = new BasePMEngineModule();
            baseModule.Registrate(services);

            return services;
        }

        public static async Task ConfigurePmEngine(this IApplicationBuilder app)
        {
            await app.ApplicationServices.GetRequiredService<PmMigrationInitializer>().Configure(app.ApplicationServices).ConfigureAwait(false);
        }

        public static void AddPmModule<T>(this IServiceCollection services) where T : IModuleRegistrator
        {
            var module = Activator.CreateInstance<T>() ?? throw new NullReferenceException($"Не удалось инициализировать {typeof(T)}");
            module.Registrate(services);
        }
    }
}