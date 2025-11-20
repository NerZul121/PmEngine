using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Interfaces
{
    public interface IOutputManagerFactory
    {
        public IOutputManager CreateForUser(UserSession user);
        public Type OutputType { get; }
    }
}