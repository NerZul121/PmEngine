using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PmEngine.Core.BaseClasses;
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
    public class UserSession : IUserSession
    {
        public IOutputManager? DefaultOutput { get; set; }
        public ILogger Logger { get; set; }
        public PmEngine Engine
        {
            get
            {
                if (_engine is null)
                    _engine = Services.GetRequiredService<PmEngine>();

                return _engine;
            }
        }

        internal protected List<IOutputManager> OutputManagersCache { get; set; } = [];

        private PmEngine? _engine;

        public Arguments LocalStore { get; } = new();

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

        private ActionWrapper? _inputAction;

        /// <summary>
        /// Действие ввода информации
        /// Устанавливается, после чего введенная пользователем информация вызовет это действие и введенный текст отправится в InputData
        /// </summary>
        public ActionWrapper? InputAction
        {
            get { return _inputAction; }
            set
            {
                _inputAction = value;

                if (Engine.Properties.EnableStateless)
                {
                    Services.InContextSync(context =>
                    {
                        var userData = Reload(context);
                        userData.SessionData = JsonSerializer.Serialize(new SessionData(this));
                        context.SaveChanges();
                    });
                }
            }
        }

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
                Services.InContextSync(async (context) =>
                {
                    _cache = context.Set<UserEntity>().AsNoTracking().First(u => u.Id == Id);
                });

                return _cache;
            }
        }

        /// <summary>
        /// Кешированная информация о пользователе
        /// </summary>
        public UserEntity CachedData { get { if (_cache is null) _cache = Data; return _cache; } }

        private ActionWrapper? _currentAction;

        /// <summary>
        /// Текущее действие пользователя
        /// </summary>
        public virtual ActionWrapper? CurrentAction
        {
            get { return _currentAction; }
            set
            {
                _currentAction = value;

                if (Engine.Properties.EnableStateless)
                {
                    Services.InContextSync(context =>
                    {
                        var userData = Reload(context);
                        userData.SessionData = JsonSerializer.Serialize(new SessionData(this));
                        context.SaveChanges();
                    });
                }
            }
        }

        /// <summary>
        /// Список следующих действий пользователя
        /// </summary>
        public virtual INextActionsMarkup? NextActions
        {
            get { return _nextActions; }
            set
            {
                _nextActions = value;

                if (Engine.Properties.EnableStateless)
                {
                    Services.InContextSync(context =>
                    {
                        var userData = Reload(context);
                        userData.SessionData = JsonSerializer.Serialize(new SessionData(this));
                        context.SaveChanges();
                    });
                }
            }
        }

        private INextActionsMarkup? _nextActions;

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
        public UserSession(IServiceProvider services, UserEntity user, Action<IUserSession>? init = null)
        {
            Services = services;
            Logger = Services.GetRequiredService<ILogger>();

            Id = user.Id;
            _engine = services.GetRequiredService<PmEngine>();

            if (init != null)
                init(this);

            if (Engine.Properties.EnableStateless && !string.IsNullOrEmpty(user.SessionData))
            {
                var sessionData = JsonSerializer.Deserialize<SessionData>(user.SessionData);
                NextActions = sessionData.NextActions(services);
                sessionData.CurrentAction = sessionData.CurrentAction;
                sessionData.InputAction = sessionData.InputAction;
            }
            else
                CurrentAction = new ActionWrapper("Initialization", Engine.Properties.InitializationAction, Engine.Properties.StartArguments);

            services.GetRequiredService<IEngineProcessor>().MakeEventSync<IUserSesseionInitializeEventHandler>((handler) => handler.Handle(this));
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

        public UserEntity Reload(BaseContext context)
        {
            _cache = context.Set<UserEntity>().First(u => u.Id == Id);
            return _cache;
        }

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
                DefaultOutput = output;
            }

            return output;
        }

        public T GetOutput<T>() where T : IOutputManager, new()
        {
            var output = OutputManagersCache.FirstOrDefault(o => o.GetType() == typeof(T));
            if (output is null)
            {
                var factory = Services.GetServices<IOutputManagerFactory>().FirstOrDefault(s => s.OutputType == typeof(T)) ?? throw new Exception($"Cannot detected IOutputManagerFactory for {typeof(T)}");
                output = factory.CreateForUser(this);
                DefaultOutput = output;
            }

            return (T)output;
        }

        /// <summary>
        /// Пометить пользователя как онлайн
        /// </summary>
        public async Task MarkOnline()
        {
            await Services.GetRequiredService<IContextHelper>().InContext(async (context) =>
            {
                var user = Reload(context);
                user.LastOnlineDate = DateTime.Now;
                await context.SaveChangesAsync();
            });
        }
    }
}