using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    /// <summary>
    /// Обертка действия
    /// </summary>
    public class ActionWrapper
    {
        /// <summary>
        /// ГУИД как идентификатор действия
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Видно-ли действие пользователю
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Список следующих действий
        /// </summary>
        public INextActionsMarkup NextActions { get; set; } = new BaseMarkup();

        /// <summary>
        /// Отображаемое имя действия
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Класс действия, реализующий IAction
        /// </summary>
        public Type? ActionType { get; set; }

        /// <summary>
        /// Имя типа действия. Доступно ТОЛЬКО при UseLibStorage = true!!!
        /// </summary>
        public string? ActionTypeName { get; set; }

        /// <summary>
        /// Текст экшена
        /// </summary>
        public string? ActionText { get; set; }

        /// <summary>
        /// Аргументы
        /// </summary>
        public Arguments Arguments { get; set; } = new Arguments();

        /// <summary>
        /// Обертка для действия
        /// </summary>
        /// <param name="name"></param>
        /// <param name="actionClass"></param>
        public ActionWrapper(string name, Type? actionClass)
        {
            DisplayName = name;
            ActionType = actionClass;
        }

        /// <summary>
        /// Обертка для действия
        /// </summary>
        /// <param name="name"></param>
        public ActionWrapper(string name)
        {
            DisplayName = name;
        }

        /// <summary>
        /// Обертка для действия
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
		/// <param name="arguments"></param>
        public ActionWrapper(string displayName, Type? actionClass, Arguments arguments)
        {
            DisplayName = displayName;
            ActionType = actionClass;
            Arguments = arguments;
        }

        /// <summary>
        /// Обертка для действия
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
        /// <param name="arguments"></param>
        public ActionWrapper(string displayName, string? actionClass, Arguments? arguments = null)
        {
            DisplayName = displayName;
            ActionTypeName = actionClass;
            Arguments = arguments ?? new Arguments();
        }

        /// <summary>
        /// Преобразование в строку (Data.Name)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DisplayName;
        }
    }

    /// <summary>
    /// Сахарок для инициализации врапперов без typeof
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionWrapper<T> : ActionWrapper where T : IAction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ActionWrapper(string name = "") : base(name, typeof(T))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public ActionWrapper(string name, Arguments args) : base(name, typeof(T), args)
        {
        }
    }
}