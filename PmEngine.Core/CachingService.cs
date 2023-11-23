using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Entities.Base;
using PmEngine.Core.Interfaces;
using System.Collections.Concurrent;

namespace PmEngine.Core
{
    /// <summary>
    /// Сервис кеширования сущностей
    /// </summary>
    public class CachingService : ICachingService
    {
        private IServiceProvider _serviceProvider;
        public CachingService(IServiceProvider serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Кеш
        /// </summary>
        public static ConcurrentDictionary<Type, ConcurrentDictionary<long, BaseEntity>> CachedData { get; } = new();

        /// <summary>
        /// Расширенное кеширование сущностей
        /// </summary>
        public static ConcurrentDictionary<Type, Func<BaseContext, long, Task<BaseEntity>>> ExtandedCaching { get; } = new();

        /// <summary>
        /// Кеширование объекта
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public async Task Cache<T>(T value, long? id = null) where T : BaseEntity
        {
            if (id is null)
                id = value.Id;

            if (CachedData.TryGetValue(typeof(T), out var cachedTypeData))
            {
                cachedTypeData[id.Value] = value;
                return;
            }

            cachedTypeData = new ConcurrentDictionary<long, BaseEntity>();
            cachedTypeData[id.Value] = value;

            CachedData.TryAdd(typeof(T), cachedTypeData);
        }

        /// <summary>
        /// Загрузка кеша
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="orLoad"></param>
        /// <returns></returns>
        public async Task<T?> GetCache<T>(long id, bool orLoad = true) where T : BaseEntity
        {
            if (CachedData.TryGetValue(typeof(T), out var cachedTypeData))
            {
                if (cachedTypeData.TryGetValue(id, out var cachedData))
                    return (T)cachedData;

                if (orLoad)
                {
                    var entity = await HardLoad<T>(id);

                    if (entity != null)
                        cachedTypeData[id] = entity;

                    return entity;
                }
            }

            if (!orLoad)
                return null;

            var loadedEntity = await HardLoad<T>(id);
            if (loadedEntity is null)
                return null;

            cachedTypeData = new ConcurrentDictionary<long, BaseEntity>();
            cachedTypeData[id] = loadedEntity;

            CachedData.TryAdd(typeof(T), cachedTypeData);

            return loadedEntity;
        }

        private async Task<T?> HardLoad<T>(long id) where T : BaseEntity
        {
            T? result = null;

            await _serviceProvider.GetRequiredService<IContextHelper>().InContext(async (context) =>
            {
                if (ExtandedCaching.ContainsKey(typeof(T)))
                    result = (T)await ExtandedCaching[typeof(T)](context, id);

                result = await context.FindAsync<T>(id);
            });

            return result;
        }
    }
}