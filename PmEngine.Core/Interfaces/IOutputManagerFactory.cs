namespace PmEngine.Core.Interfaces
{
    public interface IOutputManagerFactory
    {
        public IOutputManager CreateForUser(IUserSession user);
        public Type OutputType { get; }
    }
}