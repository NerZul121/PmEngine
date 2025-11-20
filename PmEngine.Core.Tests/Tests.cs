using PmEngine.Core.Entities;
using PmEngine.Core.Tests.Entities;
using PmEngine.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;

namespace PmEngine.Core.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Testing()
        {
            var services = new ServiceCollection();
            services.AddScoped<ITester, Tester1>();
            services.AddScoped<ITester, Tester2>();
            var provider = services.BuildServiceProvider();
            var tester = provider.GetRequiredService<ITester>();
            tester.Test();
        }

        [TestMethod]
        public void BaseInitialization()
        {
            var services = new ServiceCollection();

            services.AddPMEngine(engine => { engine.DataProvider = Enums.DataProvider.InMemory; engine.InitializationAction = typeof(MyActionJoka); });
            services.AddPmModule<TestModuleRegistrator>();
            services.AddLogging();
            using ILoggerFactory loggerFactory = LoggerFactory.Create((a) => { a.AddConsole(); });
            services.AddSingleton(loggerFactory);
            services.AddSingleton<ILogger>(loggerFactory.CreateLogger(""));

            IServiceProvider provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            provider = scope.ServiceProvider;
            var engine = provider.GetRequiredService<PmMigrationInitializer>();
            engine.Configure(provider).Wait();

            UserEntity user = new();
            using var context = new PMEContext(provider.GetRequiredService<PmConfig>());
            user = new UserEntity();
            var kakob = new AuthTwo();
            context.Add(kakob);
            context.Add(user);
            context.SaveChangesAsync().ConfigureAwait(false);

            var ps = provider.GetRequiredService<ServerSession>().GetUserSession(user.Id, null, typeof(TestCoreOutput)).Result;
            Console.WriteLine(ps.NextActions);
        }

        [TestMethod]
        public void CheckDescription()
        {
            Console.WriteLine(typeof(UserEntity).GetProperty("Id")?.GetDescription());
        }

        [TestMethod]
        public void GetDefaultTest()
        {
            var dict = new Dictionary<string, object>() { { "MyID", 100 } };

            Console.WriteLine(dict.GetArgument<int>("myId"));
            Console.WriteLine(dict.GetArgument<int>("MyId"));
            Console.WriteLine(dict.GetArgument<int>("test"));
        }

        [TestMethod]
        public void Logging()
        {
        }
    }
}