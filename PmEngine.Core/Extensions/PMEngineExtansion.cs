using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Interfaces;
using System.Reflection;

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
        public static IServiceCollection AddPMEngine(this IServiceCollection services, Action<IEngineConfigurator>? action = null)
        {
            var engine = new PMEngineConfigurator();
            engine.Services = services;
            services.AddSingleton<IEngineConfigurator>(engine);
            services.AddSingleton(typeof(IEngineProcessor), typeof(EngineProcessor));
            services.AddScoped(typeof(IUserScopeData), typeof(UserScopeData));
            services.AddScoped(typeof(ILocalHelper), typeof(LocalHelper));
            services.AddSingleton(typeof(IServerSession), typeof(ServerSession));
            services.AddTransient(typeof(BaseContext), typeof(BaseContext));
            services.AddTransient(typeof(IDataContext), typeof(BaseContext));
            services.AddSingleton(typeof(IContextHelper), typeof(ContextHelper));

            if (action != null)
                action(engine);

            FindModulesFromAssemblies(services);
            return services;
        }

        /// <summary>
        /// Добавляет только контекст и сущности
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPMEngineContext(this IServiceCollection services)
        {
            services.AddTransient(typeof(BaseContext), typeof(BaseContext));
            services.AddTransient(typeof(IDataContext), typeof(BaseContext));
            services.AddSingleton(typeof(IContextHelper), typeof(ContextHelper));

            FindModulesFromAssemblies(services, typeof(IDataEntity));

            return services;
        }

        private static void FindModulesFromAssemblies(IServiceCollection Services, Type? type = null)
        {
            var types = new List<Type>();
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).ToList();
            var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

            toLoad.ForEach((path) =>
            {
                try
                {
                    loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                }
                // Безопасная загрузка при наличии работы с "мягкими" ссылками.
                catch { }
            });

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
            foreach (var assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetExportedTypes().Where(tp => !types.Contains(tp) && !tp.IsAbstract && tp.GetInterface("IModuleRegistrator") != null && (type != null ? tp.GetInterface(type.Name) != null : true)).ToList());
                }
                catch
                {
                }
            }

            foreach (var t in types)
                ((IModuleRegistrator?)Activator.CreateInstance(t))?.Registrate(Services);
        }

        public static void ConfigureEngine(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<IEngineConfigurator>().Configure(app.ApplicationServices).Wait();
        }
    }
}