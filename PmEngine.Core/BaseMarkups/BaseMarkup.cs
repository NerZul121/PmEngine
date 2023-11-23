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
        public IActionArguments Arguments { get; set; } = new ActionArguments();

        /// <summary>
        /// Базовая разметка
        /// </summary>
        public BaseMarkup() { }

        /// <summary>
        /// Базовая разметка
        /// </summary>
        /// <param name="actions"></param>
        public BaseMarkup(IEnumerable<IActionWrapper> actions)
        {
            Actions = actions.Select(a => new IActionWrapper[] { a });
        }

        /// <summary>
        /// Базовая разметка
        /// </summary>
        /// <param name="actions"></param>
        public BaseMarkup(IEnumerable<IEnumerable<IActionWrapper>> actions)
        {
            Actions = actions;
        }

        /// <summary>
        /// Действия
        /// </summary>
        public IEnumerable<IEnumerable<IActionWrapper>> Actions { get; set; } = Enumerable.Empty<IEnumerable<IActionWrapper>>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IActionWrapper> GetFloatNextActions()
        {
            return Actions.SelectMany(a => a);
        }

        /// <summary>
        /// Действия
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IEnumerable<IActionWrapper>> GetNextActions()
        {
            return Actions;
        }
    }
}