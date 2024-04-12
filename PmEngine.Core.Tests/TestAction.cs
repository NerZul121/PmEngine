using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Tests
{
    internal class TestAction : IAction
    {
        public async Task<INextActionsMarkup?> DoAction(ActionWrapper currentAction, IUserSession user)
        {
            user.OutputContent += "Привет!";
            return new SingleMarkup(new ActionWrapper[] { new ActionWrapper("Привет!", typeof(TestAction)) });
        }

        public async Task AfterAction(ActionWrapper currentAction, IUserSession user)
        {
            Console.WriteLine("TetsAfter");
        }
    }
}