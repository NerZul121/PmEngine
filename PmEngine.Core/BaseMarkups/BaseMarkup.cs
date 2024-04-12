using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseMarkups
{
    /// <summary>
    /// Базовая разметка, реализующая двумерный массив.
    /// </summary>
    public class BaseMarkup : INextActionsMarkup
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
        /// Базовая разметка
        /// </summary>
        public BaseMarkup() { }

        /// <summary>
        /// Базовая разметка
        /// </summary>
        /// <param name="actions"></param>
        public BaseMarkup(IEnumerable<ActionWrapper> actions)
        {
            Actions = actions.Select(a => new List<ActionWrapper> { a }).ToList();
        }

        /// <summary>
        /// Базовая разметка
        /// </summary>
        /// <param name="actions"></param>
        public BaseMarkup(IEnumerable<IEnumerable<ActionWrapper>> actions)
        {
            Actions = actions.Select(s => s.ToList()).ToList();
        }

        /// <summary>
        /// Действия
        /// </summary>
        public List<List<ActionWrapper>> Actions { get; set; } = new() { new() };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ActionWrapper> GetFloatNextActions()
        {
            return Actions.SelectMany(a => a);
        }

        /// <summary>
        /// Действия
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IEnumerable<ActionWrapper>> GetNextActions()
        {
            return Actions;
        }

        public void Add(ActionWrapper action)
        {
            Actions.Last().Add(action);
        }

        public ActionWrapper Add(string displayName, Type actionClass, Arguments? arguments = null)
        {
            var na = new ActionWrapper(displayName, actionClass, arguments ?? new());
            Actions.Last().Add(na);
            return na;
        }

        public ActionWrapper Add<T>(string displayName, Arguments? arguments = null) where T : ActionWrapper
        {
            var na = new ActionWrapper(displayName, typeof(T), arguments ?? new());
            Actions.Last().Add(na);
            return na;
        }
    }
}