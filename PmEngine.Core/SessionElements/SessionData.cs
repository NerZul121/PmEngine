using Microsoft.Extensions.DependencyInjection;
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
        public string ActionName { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<String, Object> Arguments { get; set; }

        public ActionWrapper Wrap(IServiceProvider serviceProvider)
        {
            var result = new ActionWrapper(DisplayName, serviceProvider.GetServices<IAction>().FirstOrDefault(a => a.GetType().ToString() == ActionName).GetType(), new ActionArguments(Arguments));

            return result;
        }

        public ActionWrapperSaveModel()
        {

        }

        public ActionWrapperSaveModel(IActionWrapper actionWrapper)
        {
            ActionName = actionWrapper.ActionType.ToString();
            DisplayName = actionWrapper.DisplayName;
            Arguments = actionWrapper.Arguments.ToDict();
        }
    }
}