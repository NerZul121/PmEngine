namespace PmEngine.Core.Interfaces
{
    public interface IOutputCreater
    {
        public IOutputManager Create(IUserSession session);
    }
}