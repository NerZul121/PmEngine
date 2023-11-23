namespace PmEngine.Core.Interfaces
{
    public interface IServerSession
    {
        public IEnumerable<IUserSession> GetAllSessions();
        public Task<IUserSession> GetUserSession(long userId, Action<IUserSession>? init = null);
        public IUserSession? TryGetUserSession(long userId);
        public Task RemoveUserSession(IUserSession session);
    }
}