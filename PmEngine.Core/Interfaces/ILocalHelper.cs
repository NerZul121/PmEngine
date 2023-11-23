namespace PmEngine.Core.Interfaces
{
    public interface ILocalHelper
    {
        public Task<string?> GetLocal(string key);
        public Task SetLocal(string key, string? value);
    }
}
