using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Tests
{
    internal class TestAction : IAction
    {
        public async Task<INextActionsMarkup?> DoAction(ActionWrapper currentAction, UserSession user)
        {
            user.OutputContent += "Привет!";
            return new SingleMarkup(new ActionWrapper[] { new ActionWrapper("Привет!", typeof(TestAction)) });
        }

        public async Task AfterAction(ActionWrapper currentAction, UserSession user)
        {
            Console.WriteLine("TetsAfter");
        }
    }
}