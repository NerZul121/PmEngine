using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Entities;
using PmEngine.Core.Extensions;
using PmEngine.Core.Interfaces;
using System.Collections.Frozen;
using System.Text.Json;

namespace PmEngine.Core.SessionElements
{
    /// <summary>
    /// Сессия пользователя
    /// </summary>
    public class UserSession : IDisposable
    {
        public IOutputManager? DefaultOutput { get; set; }
        public ILogger Logger { get; set; }

        public async Task SaveState()
        {
            using var context = new PMEContext(Services.GetRequiredService<PmConfig>(), Services);
            var userData = Reload(context);
            userData.SessionData = JsonSerializer.Serialize(new SessionData(this));
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        internal protected List<IOutputManager> OutputManagersCache { get; set; } = [];

        public Arguments LocalStore { get; } = new();

        /// <summary>
        /// Сервисы в скопе пользователя
        /// </summary>
        public IServiceProvider Services { get; private set; }

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

        private ActionWrapper? _inputAction;

        /// <summary>
        /// Действие ввода информации
        /// Устанавливается, после чего введенная пользователем информация вызовет это действие и введенный текст отправится в InputData
        /// </summary>
        public ActionWrapper? InputAction
        {
            get { return _inputAction; }
            set { _inputAction = value; }
        }

        public Task ActionProcess(ActionWrapper actionWrapper)
        {
            return Services.GetRequiredService<IActionProcessor>().ActionProcess(actionWrapper, this);
        }

        public Task<INextActionsMarkup?> MakeAction(ActionWrapper actionWrapper)
        {
            return Services.GetRequiredService<IActionProcessor>().MakeAction(actionWrapper, this);
        }

        /// <summary>
        /// Кешированная информация о пользователе
        /// </summary>
        public UserEntity CachedData { get { return _cache; } }

        private ActionWrapper? _currentAction;

        /// <summary>
        /// Текущее действие пользователя
        /// </summary>
        public virtual ActionWrapper? CurrentAction
        {
            get { return _currentAction; }
            set { _currentAction = value; }
        }

        /// <summary>
        /// Список следующих действий пользователя
        /// </summary>
        public virtual INextActionsMarkup? NextActions
        {
            get { return _nextActions; }
            set { _nextActions = value; }
        }

        private INextActionsMarkup? _nextActions;

        public string? OutputContent { get; set; }

        public IEnumerable<object>? Media { get; set; }

        /// <summary>
        /// Сессия пользователя
        /// </summary>
        /// <param name="user">ID пользователя</param>
        public UserSession(IServiceProvider services, UserEntity user)
        {
            Id = user.Id;
            Services = services;
            Logger = Services.GetRequiredService<ILoggerFactory>().CreateLogger($"User-{Id}");
            Permissions = user.Permissions?.Select(s => s.Permission).ToHashSet() ?? [];
            _cache = user;
        }

        /// <summary>
        /// Получить переменную сессии пользователя. В случае указания некорректного типа вернется default
        /// </summary>
        /// <typeparam name="T">Тип данных переменной</typeparam>
        /// <param name="name">Имя переменной</param>
        /// <returns>Значение переменной</returns>
        public T? GetLocal<T>(string name)
        {
            var value = LocalStore.Get<T>(name);
            Logger.LogInformation($"User{Id} - GetLocal {name}: {value}");
            return value;
        }

        /// <summary>
        /// Установить сессионную переменную пользователя
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <param name="value">Значение</param>
        public void SetLocal(string name, object? value)
        {
            Logger.LogInformation($"User{Id} - SetLocal {name}: {value}");
            LocalStore.Set(name, value);
        }

        public UserEntity Reload(PMEContext context)
        {
            _cache = context.Set<UserEntity>().Include(d => d.Permissions).First(u => u.Id == Id);
            Permissions = _cache.Permissions?.Select(s => s.Permission).ToHashSet() ?? [];
            return _cache;
        }

        public HashSet<string> Permissions { get; private set; } = [];

        public IOutputManager Output { get { return DefaultOutput ?? throw new Exception("Output not setted."); } }

        public void AddToOutput(string add)
        {
            if (String.IsNullOrEmpty(OutputContent))
                OutputContent += add;
            else
                OutputContent += Environment.NewLine + add;
        }

        public void Dispose()
        {
            Logger.LogInformation($"User{Id} - Disposing...");
        }

        public IOutputManager GetOutputOrCreate(Type outputType)
        {
            var output = OutputManagersCache.FirstOrDefault(o => o.GetType() == outputType);
            if (output is null)
            {
                var factory = Services.GetServices<IOutputManagerFactory>().FirstOrDefault(s => s.OutputType == outputType) ?? throw new Exception($"Cannot detected IOutputManagerFactory for {outputType}");
                output = factory.CreateForUser(this);

                if (DefaultOutput is null)
                    DefaultOutput = output;

                OutputManagersCache.Add(output);
            }

            return output;
        }

        public T GetOutput<T>() where T : IOutputManager
        {
            var output = OutputManagersCache.FirstOrDefault(o => o.GetType() == typeof(T));
            if (output is null)
            {
                var factory = Services.GetServices<IOutputManagerFactory>().FirstOrDefault(s => s.OutputType == typeof(T)) ?? throw new Exception($"Cannot detected IOutputManagerFactory for {typeof(T)}");
                output = factory.CreateForUser(this);

                if (DefaultOutput is null)
                    DefaultOutput = output;

                OutputManagersCache.Add(output);
            }

            return (T)output;
        }

        /// <summary>
        /// Пометить пользователя как онлайн
        /// </summary>
        public async Task MarkOnline()
        {
            using var ctx = new PMEContext(Services.GetRequiredService<PmConfig>(), Services);
            var user = Reload(ctx);
            user.LastOnlineDate = DateTime.Now;
            await ctx.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}