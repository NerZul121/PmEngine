using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Entities;
using PmEngine.Core.Enums;
using PmEngine.Core.Extensions;
using PmEngine.Core.Interfaces;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Tests
{
    public static class TestHelper
    {
        public static IServiceProvider GetProvider(Action<IServiceProvider>? afterInit = null, LogLevel logLevel = LogLevel.Error)
        {
            var services = new ServiceCollection();

            services.AddPMEngine(engine => { engine.DataProvider = DataProvider.InMemory; engine.InitializationAction = typeof(TestAction); engine.ExceptionAction = typeof(TestAction); engine.EnableStateless = true; });
            services.AddLogging();
            ILoggerFactory loggerFactory = LoggerFactory.Create((a) => { a.AddConsole(); a.SetMinimumLevel(logLevel); });
            services.AddSingleton(loggerFactory);
            services.AddSingleton<ILogger>(loggerFactory.CreateLogger(""));
            services.AddScoped(typeof(IOutputManager), typeof(TestCoreOutput));
            IServiceProvider provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();

            provider = scope.ServiceProvider;
            var engine = provider.GetRequiredService<PmMigrationInitializer>();
            engine.Configure(provider).Wait();

            if (afterInit is not null)
                afterInit(provider);

            return provider;
        }

        public static async Task<UserSession> CreateNewUser(IServiceProvider services)
        {
            var gameSession = services.GetRequiredService<ServerSession>();
            using var ctx = new PMEContext(gameSession.Config);
            var user = new UserEntity();
            user.RegistrationDate = DateTime.Now;
            user.LastOnlineDate = DateTime.Now;
            ctx.Add(user);
            ctx.SaveChanges();

            return await gameSession.GetUserSession(user.Id).ConfigureAwait(false);
        }
    }
}
