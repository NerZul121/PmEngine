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
            Actions = actions.Select(a => new ActionWrapper[] { a });
        }

        /// <summary>
        /// Базовая разметка
        /// </summary>
        /// <param name="actions"></param>
        public BaseMarkup(IEnumerable<IEnumerable<ActionWrapper>> actions)
        {
            Actions = actions;
        }

        /// <summary>
        /// Действия
        /// </summary>
        public IEnumerable<IEnumerable<ActionWrapper>> Actions { get; set; } = Enumerable.Empty<IEnumerable<ActionWrapper>>();

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
    }
}