using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Tests
{
    internal class MyActionJoka : IAction
    {
        public async Task<INextActionsMarkup?> DoAction(IActionWrapper currentAction, IUserSession userId, IActionArguments arguments)
        {
            return null;
        }

        public async Task AfterAction(IActionWrapper currentAction, IUserSession user, IActionArguments arguments)
        {
            Console.WriteLine("TetsAfter");
        }
    }
}