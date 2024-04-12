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
        public Arguments Arguments { get; set; } = new Arguments();

        /// <summary>
        /// Разметка кнопок в один столбец
        /// </summary>
        public SingleMarkup() { }

        /// <summary>
        /// Разметка кнопок в один столбец с инициализацией действий
        /// </summary>
        /// <param name="actions">Действия</param>
        public SingleMarkup(IEnumerable<ActionWrapper> actions)
        {
            Actions = actions.ToList();
        }

        /// <summary>
        /// Действия
        /// </summary>
        public List<ActionWrapper> Actions { get; set; } = new();

        /// <summary>
        /// Получение действий в двумерном массиве
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<ActionWrapper>> GetNextActions()
        {
            return Actions.Select(s => new ActionWrapper[] { s });
        }

        /// <summary>
        /// Добавить действие
        /// </summary>
        /// <param name="action"></param>
        public void Add(ActionWrapper action)
        {
            Actions.Add(action);
        }

        /// <summary>
        /// Добавить действие
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
        /// <param name="arguments"></param>
        public ActionWrapper Add(string displayName, Type actionClass, Arguments? arguments = null)
        {
            var ar = arguments is null ? new ActionWrapper(displayName, actionClass) : new ActionWrapper(displayName, actionClass, arguments);
            Actions.Add(ar);
            return ar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ActionWrapper> GetFloatNextActions()
        {
            return Actions.ToArray();
        }
    }
}