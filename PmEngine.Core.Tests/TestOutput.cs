using PmEngine.Core.Interfaces;
using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Tests
{
    internal class TestCoreOutput : IOutputManager
    {
        private int _counter = 0;
        private UserSession _user;

        public TestCoreOutput(UserSession user)
        {
            _user = user;
        }

        public async Task DeleteMessage(int messageId)
        {
            Console.WriteLine($"{_user.Id}: Deleted Message #{messageId}");
        }

        public void DeleteMessageSafe(int messageId)
        {
            Console.WriteLine($"{_user.Id}: Deleted safe Message #{messageId}");
        }

        public async Task EditContent(int messageId, string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, Arguments? additionals = null)
        {
            Console.WriteLine($"{_user.Id}: Edit Mesage content Id {messageId} on {content}");
        }

        public void EditContentSafe(int messageId, string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, Arguments? additionals = null)
        {
            Console.WriteLine($"{_user.Id}: Edit safe Mesage content Id {messageId} on {content}");
        }

        public async Task<int> ShowContent(string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, Arguments? additionals = null)
        {
            Console.WriteLine($"{_user.Id}: Send content {content}");
            return _counter++;
        }

        public void ShowContentSafe(string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, Arguments? additionals = null)
        {
            Console.WriteLine($"{_user.Id}: Send safe content {content}");
            _counter++;
        }
    }

    internal class TestCoreOutputFactory : IOutputManagerFactory
    {
        public Type OutputType => typeof(TestCoreOutput);

        public IOutputManager CreateForUser(UserSession user)
        {
            return new TestCoreOutput(user);
        }
    }
}