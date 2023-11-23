using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    /// <summary>
    /// Обертка действия
    /// </summary>
    public class ActionWrapper : IActionWrapper
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
		/// Текст экшена
		/// </summary>
		public string? ActionText { get; set; }

		/// <summary>
		/// Аргументы
		/// </summary>
		public IActionArguments Arguments { get; set; } = new ActionArguments();

        /// <summary>
        /// Обертка для действия
        /// </summary>
        /// <param name="name"></param>
        /// <param name="actionClass"></param>
        public ActionWrapper(string name, Type? actionClass = null)
		{
			DisplayName = name;
			ActionType = actionClass;
		}

        /// <summary>
        /// Обертка для действия
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
		/// <param name="arguments"></param>
        public ActionWrapper(string displayName, Type? actionClass, IActionArguments arguments)
        {
            DisplayName = displayName;
            ActionType = actionClass;
            Arguments = arguments;
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
}