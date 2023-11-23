using PmEngine.Core.Entities.Base;

namespace PmEngine.Core.Interfaces
{
    public interface ICachingService
    {
        public Task Cache<T>(T value, long? id = null) where T : BaseEntity;
        public Task<T?> GetCache<T>(long id, bool orLoad = true) where T : BaseEntity;
    }
}