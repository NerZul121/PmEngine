using PmEngine.Core.Interfaces;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Tests
{
    internal class MyActionJoka : IAction
    {
        public async Task<INextActionsMarkup?> DoAction(ActionWrapper currentAction, UserSession userId)
        {
            return null;
        }

        public async Task AfterAction(ActionWrapper currentAction, UserSession user)
        {
            Console.WriteLine("TetsAfter");
        }
    }
}