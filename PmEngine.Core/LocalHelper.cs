using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Entities;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    /// <summary>
    /// Класс, предоставляющий функционал к управлению постоянными переменными пользователей
    /// </summary>
    public class LocalHelper : ILocalHelper
    {
        private IServiceProvider _serviceProvider;
        public LocalHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Получение информации о постоянных переменных пользователя
        /// </summary>
        /// <param name="localName">Имя переменной</param>
        /// <returns>Значение переменной</returns>
        public async Task<string?> GetLocal(string localName, long userId)
        {
            string? result = null;
            await _serviceProvider.GetRequiredService<IContextHelper>().InContext(async (context) =>
            {
                result = (await context.Set<UserLocalEntity>().FirstOrDefaultAsync(p => p.UserId == userId && p.Name == localName))?.Value;
            });
            return result;
        }

        /// <summary>
        /// Установка постоянной переменной пользователя
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="key">Ключ</param>
        public async Task SetLocal(string key, string? value, long userId)
        {
            await _serviceProvider.GetRequiredService<IContextHelper>().InContext(async (context) =>
            {
                var local = await context.Set<UserLocalEntity>().FirstOrDefaultAsync(p => p.UserId == userId && p.Name == key);

                if (local is null)
                {
                    if (String.IsNullOrEmpty(value))
                        return;

                    var newLocal = new UserLocalEntity() { UserId = userId, Name = key, Value = value };
                    await context.Set<UserLocalEntity>().AddAsync(newLocal);
                }
                else
                {
                    if (String.IsNullOrEmpty(value))
                        context.Set<UserLocalEntity>().Remove(local);
                    else
                        local.Value = value;
                }

                await context.SaveChangesAsync();
            });
        }
    }
}