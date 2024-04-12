using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Tests
{
    internal class MyActionJoka : IAction
    {
        public async Task<INextActionsMarkup?> DoAction(ActionWrapper currentAction, IUserSession userId)
        {
            return null;
        }

        public async Task AfterAction(ActionWrapper currentAction, IUserSession user)
        {
            Console.WriteLine("TetsAfter");
        }
    }
}