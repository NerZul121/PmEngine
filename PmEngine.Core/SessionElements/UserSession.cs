﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Entities;
using PmEngine.Core.Extensions;
using PmEngine.Core.Interfaces;
using PmEngine.Core.Interfaces.Events;
using System.Text.Json;

namespace PmEngine.Core.SessionElements
{
    /// <summary>
    /// Сессия пользователя
    /// </summary>
    public partial class UserSession : IUserSession
    {
        public IOutputManager? DefaultOutput { get; set; }

        public IServiceScope Scope { get; set; }

        /// <summary>
        /// Сервисы в скопе пользователя
        /// </summary>
        public IServiceProvider Services { get; set; }

        /// <summary>
        /// Время создания сессии
        /// </summary>
        public DateTime SessionCreateTime { get; } = DateTime.Now;

        /// <summary>
        /// Кеш
        /// </summary>
        protected UserEntity? _cache;

        /// <summary>
        /// ID пользователя в БД
        /// </summary>
        public long Id;

        /// <summary>
        /// Действие ввода информации
        /// Устанавливается, после чего введенная пользователем информация вызовет это действие и введенный текст отправится в InputData
        /// </summary>
        public IActionWrapper? InputAction { get; set; }

        /// <summary>
        /// Информация о пользователе из БД. Загружается при каждом (!) вызове. Если нужно вызвать более одного раза, считайте это в отдельную переменную и используйте её или CachedData.
        /// Пример использования:
        /// var user = userSession.Data;
        /// if(user.CurrentHP + user.CurrentMP + user.Level + user.CurrentExp > 0) ...
        /// </summary>
        public UserEntity Data
        {
            get
            {
                Services.GetRequiredService<IContextHelper>().InContext(async (context) =>
                {
                    _cache = context.Set<UserEntity>().AsNoTracking().First(u => u.Id == Id);
                }).Wait();

                return _cache;
            }
        }

        /// <summary>
        /// Кешированная информация о пользователе
        /// </summary>
        public UserEntity CachedData { get { if (_cache is null) _cache = Data; return _cache; } }

        /// <summary>
        /// Текущее действие пользователя
        /// </summary>
        public virtual IActionWrapper? CurrentAction { get; set; }

        /// <summary>
        /// Список следующих действий пользователя
        /// </summary>
        public virtual INextActionsMarkup? NextActions { get; set; }

        /// <summary>
        /// Временные переменные, подобно Cookies. Очищаются вместе с сессией.
        /// </summary>
        public Dictionary<string, object> Locals { get; set; } = new();

        public string? OutputContent { get; set; }

        public IEnumerable<object>? Media { get; set; }

        /// <summary>
        /// Сессия пользователя
        /// </summary>
        /// <param name="user">ID пользователя</param>
        public UserSession(IServiceProvider services, UserEntity user)
        {
            Scope = services.CreateScope();
            Services = Scope.ServiceProvider;
            var scopeData = Services.GetRequiredService<IUserScopeData>();
            scopeData.Owner = this;
            scopeData.Services = Services;

            Id = user.Id;
            var engine = services.GetRequiredService<IEngineConfigurator>();

            if (engine.Properties.EnableStateless && !string.IsNullOrEmpty(user.SessionData))
            {
                var sessionData = JsonSerializer.Deserialize<SessionData>(user.SessionData);
                NextActions = sessionData.NextActions(services);
            }
            else
                CurrentAction = new ActionWrapper("Initialization", engine.Properties.InitializationAction ?? throw new Exception("Не указано действие инициализации пользователя."), engine.Properties.StartArguments);

            services.GetRequiredService<IEngineProcessor>().MakeEvent<IUserSesseionInitializeEventHandler>(async (handler) => await handler.Handle(this)).Wait();
        }

        /// <summary>
        /// Получить переменную сессии пользователя. В случае указания некорректного типа вернется default
        /// </summary>
        /// <typeparam name="T">Тип данных переменной</typeparam>
        /// <param name="name">Имя переменной</param>
        /// <returns>Значение переменной</returns>
        public T? GetLocal<T>(string name)
        {
            try
            {
                if (Locals.ContainsKey(name))
                    return (T)Locals[name];
            }
            catch { }

            return default;
        }

        /// <summary>
        /// Установить сессионную переменную пользователя
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <param name="value">Значение</param>
        public void SetLocal(string name, object? value)
        {
            if (value is null)
                try
                {
                    Locals.Remove(name);
                }
                catch { }
            else
                Locals[name] = value;
        }

        public async Task<UserEntity> Reload(BaseContext context)
        {
            _cache = await context.Set<UserEntity>().FirstAsync(u => u.Id == Id);
            return _cache;
        }

        public T GetOutput<T>() where T : IOutputManager
        {
            var output = Services.GetRequiredService<T>();
            return output;
        }

        public IOutputManager GetOutput()
        {
            if (DefaultOutput is null)
            {
                var output = Services.GetRequiredService<IOutputManager>();
                return output;
            }

            return DefaultOutput;
        }

        public IOutputManager Output { get { return GetOutput(); } }

        public void AddToOutput(string add)
        {
            if (String.IsNullOrEmpty(OutputContent))
                OutputContent += add;
            else
                OutputContent += Environment.NewLine + add;
        }

        public void Dispose()
        {
            Scope.Dispose();
        }

        public void SetDefaultOutput<T>() where T : IOutputManager
        {
            DefaultOutput = Services.GetRequiredService<T>();
        }

        /// <summary>
        /// Пометить пользователя как онлайн
        /// </summary>
        public async Task MarkOnline()
        {
            await Services.GetRequiredService<IContextHelper>().InContext(async (context) =>
            {
                var user = await context.Set<UserEntity>().FirstAsync(p => p.Id == Id);
                user.LastOnlineDate = DateTime.Now;
                await context.SaveChangesAsync();
            });
        }
    }
}