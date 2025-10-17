namespace PmEngine.Core.Interfaces
{
    public interface ILocalHelper
    {
        public Task<string?> GetLocal(string key, long userId);
        public Task SetLocal(string key, string? value, long userId);
    }
}
