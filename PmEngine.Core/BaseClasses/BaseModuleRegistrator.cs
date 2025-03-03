using Microsoft.Extensions.DependencyInjection;
using PmEngine.Core.Interfaces;
using PmEngine.Core.Interfaces.Events;

namespace PmEngine.Core.BaseClasses
{
    /// <summary>
    /// Базовый регистратор модуля.
    /// Абстрактный класс, выполняющий базовые функции регистрация модуля.
    /// Регистрирует автоматически реализации следующих интерфейсов:
    /// IDaemon
    /// ICommand
    /// IEventHandler
    /// IAction
    /// IDataEntity
    /// </summary>
    public class BaseModuleRegistrator : IModuleRegistrator
    {
        /// <summary>
        /// Базовая регистрация
        /// </summary>
        /// <param name="services">Список сервисов</param>
        public virtual void Registrate(IServiceCollection services)
        {
            var allTypes = GetType().Assembly.GetTypes().Where(s => !s.IsAbstract && !s.IsInterface && s != null);
            var interfaces = new Type[] { typeof(IDaemon), typeof(ICommand), typeof(IManager), typeof(IEventHandler), typeof(IAction), typeof(IDataEntity), typeof(IOutputManager), typeof(ITextRefactor) };

            foreach (var i in interfaces)
                RegistrateInterfaceImplemetions(i, allTypes, services);

            AdditionalRegistrate(services, allTypes);
        }

        /// <summary>
        /// Дополнительная регистрация, если не хватает базовой.
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="allTypes">Список всех типов в текущей сборке</param>
        public virtual void AdditionalRegistrate(IServiceCollection services, IEnumerable<Type> allTypes)
        {
        }

        /// <summary>
        /// Регистрация типов, реализующих интерфейс в список сервисов движка. Проверка на реализацию интефрейса встроена.
        /// </summary>
        /// <param name="types">Список всех типов, которые надо зарегистрировать</param>
        /// <param name="interfacetype">Тип интерфейса</param>
        /// <param name="services">Коллекция сервисов</param>
        public static void RegistrateInterfaceImplemetions(Type interfacetype, IEnumerable<Type> types, IServiceCollection services)
        {
            var typesToReg = types.Where(s => s.GetInterfaces().FirstOrDefault(i => i == interfacetype) != null);

            foreach (var type in typesToReg)
            {
                try
                {
                    if (interfacetype == typeof(IDataContext))
                    {
                        services.AddTransient(typeof(IDataContext), type);
                        continue;
                    }

                    if (interfacetype == typeof(IOutputManager))
                    {
                        services.AddScoped(typeof(IOutputManager), type);
                        continue;
                    }

                    services.AddSingleton(interfacetype, type);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка иницаилазации {interfacetype} - {type}: {ex}");
                }
            }
        }

        /// <summary>
        /// Регистрация интерфеса с определенным времением жизни
        /// </summary>
        /// <param name="interfacetype"></param>
        /// <param name="types"></param>
        /// <param name="services"></param>
        /// <param name="lifetime"></param>
        public static void RegistrateInterfaceImplemetions(Type interfacetype, IEnumerable<Type> types, IServiceCollection services, ServiceLifetime lifetime)
        {
            var typesToReg = types.Where(s => s.GetInterfaces().FirstOrDefault(i => i == interfacetype) != null);

            foreach (var type in typesToReg)
            {
                try
                {
                    switch (lifetime)
                    {
                        case ServiceLifetime.Transient:
                            services.AddTransient(interfacetype, type); break;
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(interfacetype, type); break;
                        case ServiceLifetime.Scoped:
                            services.AddScoped(interfacetype, type); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка иницаилазации {interfacetype} - {type}: {ex}");
                }
            }
        }
    }
}