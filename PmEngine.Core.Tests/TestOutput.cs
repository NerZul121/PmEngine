using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Tests
{
    internal class TestCoreOutput : IOutputManager
    {
        private int _counter = 0;
        private IUserScopeData _userData;

        public TestCoreOutput(IUserScopeData userData)
        {
            _userData = userData;
        }

        public async Task DeleteMessage(int messageId)
        {
            Console.WriteLine($"{_userData.Owner.Id}: Deleted Message #{messageId}");
        }

        public async Task EditContent(int messageId, string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, IActionArguments? additionals = null)
        {
            Console.WriteLine($"{_userData.Owner.Id}: Edit Mesage content Id {messageId} on {content}");
        }

        public async Task<int> ShowContent(string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, IActionArguments? additionals = null)
        {
            Console.WriteLine($"{_userData.Owner.Id}: Send content {content}");
            return _counter++;
        }
    }
}
