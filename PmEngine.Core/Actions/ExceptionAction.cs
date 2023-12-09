using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Actions
{
    public class ExceptionAction : IAction
    {
        public async Task<INextActionsMarkup?> DoAction(IActionWrapper currentAction, IUserSession user, IActionArguments arguments)
        {
            user.AddToOutput("Sorry, but an error occurred. Report this to administrator.");
            user.AddToOutput(arguments.InputData ?? "");
            return new SingleMarkup(new [] { new ActionWrapper("Menue", user.Services.GetRequiredService<IEngineConfigurator>().Properties.InitializationAction) });
        }
    }
}