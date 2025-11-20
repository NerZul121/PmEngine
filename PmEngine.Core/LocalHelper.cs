using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Entities;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    /// <summary>
    /// Класс, предоставляющий функционал к управлению постоянными переменными пользователей
    /// </summary>
    public class LocalHelper : ILocalHelper
    {
        private PmConfig _config;

        public LocalHelper(PmConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Получение информации о постоянных переменных пользователя
        /// </summary>
        /// <param name="localName">Имя переменной</param>
        /// <returns>Значение переменной</returns>
        public async Task<string?> GetLocal(string localName, long userId)
        {
            using var context = new PMEContext(_config);
            var result = (await context.Set<UserLocalEntity>().FirstOrDefaultAsync(p => p.UserId == userId && p.Name == localName).ConfigureAwait(false))?.Value;
            return result;
        }

        /// <summary>
        /// Установка постоянной переменной пользователя
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="key">Ключ</param>
        public async Task SetLocal(string key, string? value, long userId)
        {
            using var ctx = new PMEContext(_config);
            var local = await ctx.Set<UserLocalEntity>().FirstOrDefaultAsync(p => p.UserId == userId && p.Name == key).ConfigureAwait(false);

            if (local is null)
            {
                if (String.IsNullOrEmpty(value))
                    return;

                var newLocal = new UserLocalEntity() { UserId = userId, Name = key, Value = value };
                await ctx.Set<UserLocalEntity>().AddAsync(newLocal).ConfigureAwait(false);
            }
            else
            {
                if (String.IsNullOrEmpty(value))
                    ctx.Set<UserLocalEntity>().Remove(local);
                else
                    local.Value = value;
            }

            await ctx.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}