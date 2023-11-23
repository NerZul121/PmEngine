namespace PmEngine.Core.Interfaces
{
    public interface IUserScopeData
    {
        public IUserSession Owner { get; set; }
        public IServiceProvider Services { get; set; }
    }
}