using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseMarkups
{
    public class LinedMarkup : INextActionsMarkup
    {
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
        public IActionArguments Arguments { get; set; } = new ActionArguments();

        public List<List<IActionWrapper>> Actions { get; set; } = new List<List<IActionWrapper>>();

        public void AddLine(List<IActionWrapper> actions)
        {
            Actions.Add(actions);
        }

        public void CreateLine()
        {
            Actions.Add(new List<IActionWrapper>());
        }

        public void AddToLine(IActionWrapper action)
        {
            Actions.Last().Add(action);
        }

        public IEnumerable<IActionWrapper> GetFloatNextActions()
        {
            return Actions.SelectMany(a => a);
        }

        public IEnumerable<IEnumerable<IActionWrapper>> GetNextActions()
        {
            return Actions;
        }

        public IActionWrapper Add(string displayName, Type actionClass, IActionArguments? arguments = null)
        {
            var ar = arguments is null ? new ActionWrapper(displayName, actionClass) : new ActionWrapper(displayName, actionClass, arguments);
            Actions.Last().Add(ar);
            return ar;
        }
    }
}
