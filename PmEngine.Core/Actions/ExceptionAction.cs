using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Actions
{
    /// <summary>
    /// Defdault exception action
    /// </summary>
    public class ExceptionAction : IAction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentAction"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<INextActionsMarkup?> DoAction(ActionWrapper currentAction, IUserSession user)
        {
            user.AddToOutput("Sorry, but an error occurred. Report this to administrator.");
            user.AddToOutput(currentAction.Arguments.InputData ?? "");
            var props = user.Services.GetRequiredService<IEngineConfigurator>().Properties;

            if (string.IsNullOrEmpty(props.InitializationActionName))
                return new SingleMarkup(new[] { new ActionWrapper("Menue", props.InitializationAction) });
            else
                return new SingleMarkup(new[] { new ActionWrapper("Menue", props.InitializationActionName) });
        }
    }
}