using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    public class UserScopeData : IUserScopeData
    {
        public IUserSession Owner { get; set; }
        public IServiceProvider Services { get; set; }
    }
}