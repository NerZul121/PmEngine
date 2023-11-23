using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseMarkups
{
    public class MenueMarkup : INextActionsMarkup
    {
        /// <summary>
        /// InLine мод клавиатуры
        /// </summary>
        public bool InLine { get; set; }

        /// <summary>
        /// Аргументы
        /// </summary>
        public IActionArguments Arguments { get; set; } = new ActionArguments();

        public INextActionsMarkup Header { get; set; }
        public INextActionsMarkup Body { get; set; }
        public INextActionsMarkup Footer { get; set; }

        public IEnumerable<IEnumerable<IActionWrapper>> GetNextActions()
        {
            var result = new List<IEnumerable<IActionWrapper>>();
            result.AddRange(Header.GetNextActions());
            result.AddRange(Body.GetNextActions());
            result.AddRange(Footer.GetNextActions());
            return result;
        }

        public IEnumerable<IActionWrapper> GetFloatNextActions()
        {
            var result = new List<IActionWrapper>();
            result.AddRange(Header.GetFloatNextActions());
            result.AddRange(Body.GetFloatNextActions());
            result.AddRange(Footer.GetFloatNextActions());
            return result;
        }

        public MenueMarkup()
        {
        }

        public MenueMarkup(INextActionsMarkup header, INextActionsMarkup body, INextActionsMarkup footer)
        {
            Header = header;
            Body = body;
            Footer = footer;
        }
    }
}