using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Tests
{
    internal class TestAction : IAction
    {
        public async Task<INextActionsMarkup?> DoAction(IActionWrapper currentAction, IUserSession user, IActionArguments arguments)
        {
            user.OutputContent += "Привет!";
            return new SingleMarkup(new ActionWrapper[] { new ActionWrapper("Привет!", typeof(TestAction)) });
        }

        public async Task AfterAction(IActionWrapper currentAction, IUserSession user, IActionArguments arguments)
        {
            Console.WriteLine("TetsAfter");
        }
    }
}