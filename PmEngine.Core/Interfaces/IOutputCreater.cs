using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Interfaces
{
    public interface IOutputCreater
    {
        public IOutputManager Create(UserSession session);
    }
}