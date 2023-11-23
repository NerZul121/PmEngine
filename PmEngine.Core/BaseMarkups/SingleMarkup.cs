using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseMarkups
{
    /// <summary>
    /// Разметка кнопок в один столбец
    /// </summary>
    public class SingleMarkup : INextActionsMarkup
    {
        /// <summary>
        /// InLine мод клавиатуры
        /// </summary>
        public bool InLine { get; set; }

        /// <summary>
        /// Аргументы
        /// </summary>
        public IActionArguments Arguments { get; set; } = new ActionArguments();

        /// <summary>
        /// Разметка кнопок в один столбец
        /// </summary>
        public SingleMarkup() { }

        /// <summary>
        /// Разметка кнопок в один столбец с инициализацией действий
        /// </summary>
        /// <param name="actions">Действия</param>
        public SingleMarkup(IEnumerable<IActionWrapper> actions)
        {
            Actions = actions.ToList();
        }

        /// <summary>
        /// Действия
        /// </summary>
        public List<IActionWrapper> Actions { get; set; } = new();

        /// <summary>
        /// Получение действий в двумерном массиве
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<IActionWrapper>> GetNextActions()
        {
            return Actions.Select(s => new IActionWrapper[] { s });
        }

        /// <summary>
        /// Добавить действие
        /// </summary>
        /// <param name="action"></param>
        public void Add(IActionWrapper action)
        {
            Actions.Add(action);
        }

        /// <summary>
        /// Добавить действие
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
        /// <param name="arguments"></param>
        public IActionWrapper Add(string displayName, Type actionClass, IActionArguments? arguments = null)
        {
            var ar = arguments is null ? new ActionWrapper(displayName, actionClass) : new ActionWrapper(displayName, actionClass, arguments);
            Actions.Add(ar);
            return ar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IActionWrapper> GetFloatNextActions()
        {
            return Actions.ToArray();
        }
    }
}