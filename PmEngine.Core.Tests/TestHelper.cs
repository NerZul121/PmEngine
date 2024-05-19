using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmEngine.Core.Entities;
using PmEngine.Core.Enums;
using PmEngine.Core.Extensions;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Tests
{
    public static class TestHelper
    {
        public static IServiceProvider GetProvider(Action<IServiceProvider>? afterInit = null, LogLevel logLevel = LogLevel.Error)
        {
            var services = new ServiceCollection();

            services.AddPMEngine(engine => { engine.Properties.DataProvider = DataProvider.InMemory; engine.Properties.InitializationAction = typeof(TestAction); engine.Properties.ExceptionAction = typeof(TestAction); engine.Properties.EnableStateless = true; });
            services.AddLogging();
            ILoggerFactory loggerFactory = LoggerFactory.Create((a) => { a.AddConsole(); a.SetMinimumLevel(logLevel); });
            services.AddSingleton(loggerFactory);
            services.AddSingleton<ILogger>(loggerFactory.CreateLogger(""));
            services.AddScoped(typeof(IOutputManager), typeof(TestCoreOutput));
            IServiceProvider provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();

            provider = scope.ServiceProvider;
            var engine = provider.GetRequiredService<IEngineConfigurator>();
            engine.Configure(provider).Wait();

            if (afterInit is not null)
                afterInit(provider);

            return provider;
        }

        public static async Task<IUserSession> CreateNewUser(IServiceProvider services)
        {
            var gameSession = services.GetRequiredService<IServerSession>();
            var user = await services.InContext(async (context) =>
            {
                var usr = new UserEntity();
                context.Add(usr);
                context.SaveChanges();
                return usr;
            });

            return await gameSession.GetUserSession(user.Id);
        }
    }
}
