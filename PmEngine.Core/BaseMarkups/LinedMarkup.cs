using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseMarkups
{
    /// <summary>
    /// Markup with lines
    /// </summary>
    public class LinedMarkup : INextActionsMarkup
    {
        /// <summary>
        /// Markup with lines
        /// </summary>
        public LinedMarkup()
        {
            CreateLine();
        }

        /// <summary>
        /// InLine мод клавиатуры
        /// </summary>
        public bool InLine { get; set; }

        /// <summary>
        /// Аргументы
        /// </summary>
        public Arguments Arguments { get; set; } = new Arguments();

        /// <summary>
        /// Actions
        /// </summary>
        public List<List<ActionWrapper>> Actions { get; set; } = new List<List<ActionWrapper>>();

        /// <summary>
        /// Add line with actions
        /// </summary>
        /// <param name="actions"></param>
        public void AddLine(List<ActionWrapper> actions)
        {
            Actions.Add(actions);
        }

        /// <summary>
        /// Create new empty line
        /// </summary>
        public void CreateLine()
        {
            Actions.Add(new List<ActionWrapper>());
        }

        /// <summary>
        /// Add action to last line
        /// </summary>
        /// <param name="action"></param>
        public void AddToLine(ActionWrapper action)
        {
            Actions.Last().Add(action);
        }

        /// <summary>
        /// Float actions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ActionWrapper> GetFloatNextActions()
        {
            return Actions.SelectMany(a => a);
        }

        /// <summary>
        /// Actions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<ActionWrapper>> GetNextActions()
        {
            return Actions;
        }

        /// <summary>
        /// Add action to last line
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ActionWrapper Add(string displayName, Type actionClass, Arguments? arguments = null)
        {
            var ar = arguments is null ? new ActionWrapper(displayName, actionClass) : new ActionWrapper(displayName, actionClass, arguments);
            Actions.Last().Add(ar);
            return ar;
        }
    }
}
