using PmEngine.Core.Entities;
using PmEngine.Core.Tests.Entities;
using PmEngine.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace PmEngine.Core.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestString()
        {
            var data = "YXV0aF9kYXRlPTE2OTQ1MjMwMjMKcXVlcnlfaWQ9QUFHR1VQMFFBQUFBQUlaUV9SRGFHWTh2CnVzZXI9eyJpZCI6Mjg1MDM2Njc4LCJmaXJzdF9uYW1lIjoiSmVueWEiLCJsYXN0X25hbWUiOiIiLCJ1c2VybmFtZSI6ImViYWNoOTAyMTAiLCJsYW5ndWFnZV9jb2RlIjoicnUiLCJpc19wcmVtaXVtIjp0cnVlLCJhbGxvd3Nfd3JpdGVfdG9fcG0iOnRydWV9";

            var stringedData = Encoding.UTF8.GetString(Convert.FromBase64String(data));
            var id = stringedData.Split('=').Last().Replace("{\"id\":", "").Split(':').First().Split(',').First().TrimEnd().Replace("\"", "");

            Console.WriteLine(id);
        }

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
            services.AddSingleton<IModuleRegistrator>(new TestModuleRegistrator());
            services.AddScoped(typeof(IOutputManager), typeof(TestCoreOutput));

            services.AddPMEngine(engine => { engine.Properties.DataProvider = Enums.DataProvider.InMemory; engine.Properties.InitializationAction = typeof(MyActionJoka); });
            services.AddLogging();
            using ILoggerFactory loggerFactory = LoggerFactory.Create((a) => { a.AddConsole(); });
            services.AddSingleton(loggerFactory);
            services.AddSingleton<ILogger>(loggerFactory.CreateLogger(""));

            IServiceProvider provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            provider = scope.ServiceProvider;
            var engine = provider.GetRequiredService<IEngineConfigurator>();
            engine.Configure(provider).Wait();

            UserEntity user = new();
            provider.GetRequiredService<IContextHelper>().InContext(async (context) =>
            {
                user = new UserEntity();
                var kakob = new AuthTwo();
                context.Add(kakob);
                context.Add(user);
                await context.SaveChangesAsync();
            }).Wait();

            var ps = provider.GetRequiredService<IServerSession>().GetUserSession(user.Id).Result;
            Console.WriteLine(ps.NextActions);
        }

        [TestMethod]
        public void CheckDescription()
        {
            Console.WriteLine(typeof(UserEntity).GetProperty("Id").GetDescription());
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