using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.SessionElements
{
    public class SessionData
    {
        public IEnumerable<IEnumerable<ActionWrapperSaveModel>>? Actions { get; set; }

        public INextActionsMarkup? NextActions(IServiceProvider serviceProvider)
        {
            if (Actions is null)
                return null;

            return new BaseMarkup(Actions.Select(s => s.Select(b => b.Wrap(serviceProvider))));
        }

        public SessionData()
        {

        }

        public SessionData(INextActionsMarkup? actions)
        {
            if (actions is null)
                Actions = null;

            Actions = actions.GetNextActions().Select(s => s.Select(a => new ActionWrapperSaveModel(a)));
        }
    }

    public class ActionWrapperSaveModel
    {
        public string? ActionName { get; set; }
        public string DisplayName { get; set; }
        public string GUID { get; set; }

        public Dictionary<String, Object> Arguments { get; set; }

        public ActionWrapper Wrap(IServiceProvider serviceProvider)
        {
            var result = new ActionWrapper(DisplayName, ActionName, new Arguments(Arguments));
            result.GUID = GUID;

            return result;
        }

        public ActionWrapperSaveModel()
        {

        }

        public ActionWrapperSaveModel(ActionWrapper actionWrapper)
        {
            ActionName = actionWrapper.ActionType?.ToString() ?? actionWrapper.ActionTypeName ?? null;
            DisplayName = actionWrapper.DisplayName;
            Arguments = actionWrapper.Arguments.Source;
            GUID = actionWrapper.GUID;
        }
    }
}