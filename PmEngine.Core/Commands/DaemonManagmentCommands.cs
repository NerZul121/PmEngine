using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Daemons;
using PmEngine.Core.Interfaces;
using System.Linq;

namespace PmEngine.Core.Commands
{
    public class ReloadDaemonCommand : ICommand
    {
        public string Name => "daemon reload";

        public string CommandPattern => "daemon reload daemonname";

        public string Description => "перезагрузка указанного демона";

        public int UserType => (int)Enums.UserType.Admin;

        public async Task<bool> DoCommand(string text, IUserSession user)
        {
            var daemonName = text.Split(' ').Last();

            var daemonManager = (DaemonManager)user.Services.GetServices<IManager>().First(s => s.GetType() == typeof(DaemonManager));

            var daemonType = Assembler.Get<IDaemon>(daemonName);

            daemonManager.ReloadDaemon(daemonName);

            await user.Output.ShowContent("Демон перезагружен.");

            return true;
        }
    }

    public class DaemonsCommand : ICommand
    {
        public string Name => "daemons";

        public string CommandPattern => "daemons";

        public string Description => "список демонов";

        public int UserType => (int)Enums.UserType.Admin;

        public async Task<bool> DoCommand(string text, IUserSession user)
        {
            var daemonManager = (DaemonManager)user.Services.GetServices<IManager>().First(s => s.GetType() == typeof(DaemonManager));

            await user.Output.ShowContent($"Список демонов:{Environment.NewLine}{string.Join(Environment.NewLine, daemonManager.Daemons.Select(d => $"{d.GetType().FullName}"))}");

            return true;
        }
    }
}