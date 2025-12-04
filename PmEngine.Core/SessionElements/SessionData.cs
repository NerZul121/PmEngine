using PmEngine.Core.BaseMarkups;
using PmEngine.Core.Interfaces;
using System.Reflection;
using PmEngine.Core;

namespace PmEngine.Core.SessionElements
{
    public class SessionData
    {
        public IEnumerable<IEnumerable<ActionWrapperSaveModel>>? Actions { get; set; }
        public ActionWrapperSaveModel? InputAction { get; set; }
        public ActionWrapperSaveModel? CurrentAction { get; set; }

        public INextActionsMarkup? NextActions(IServiceProvider serviceProvider)
        {
            if (Actions is null)
                return null;

            return new BaseMarkup(Actions.Select(s => s.Select(b => b.Wrap(serviceProvider))));
        }

        public SessionData()
        {
        }

        public SessionData(UserSession user)
        {
            Actions = user.NextActions?.GetNextActions().Select(s => s.Select(a => new ActionWrapperSaveModel(a)).ToList()).ToList();
            CurrentAction = user.CurrentAction is null ? null : new ActionWrapperSaveModel()
            {
                DisplayName = user.CurrentAction.DisplayName,
                ActionName = user.CurrentAction.ActionType?.ToString() ?? user.CurrentAction.ActionTypeName ?? null,
                GUID = user.CurrentAction.GUID,
                Arguments = FilterSerializableArguments(user.CurrentAction.Arguments?.Source ?? new Dictionary<string, object>())
            };
            InputAction = user.InputAction is null ? null : new ActionWrapperSaveModel(user.InputAction);
        }

        internal static Dictionary<string, object> FilterSerializableArguments(Dictionary<string, object> source)
        {
            var result = new Dictionary<string, object>();
            
            foreach (var kvp in source)
            {
                var value = FilterValue(kvp.Value);
                if (value != null)
                {
                    result[kvp.Key] = value;
                }
            }
            
            return result;
        }

        private static object? FilterValue(object? value)
        {
            if (value is null)
                return null;
            
            if (value is Exception ex)
                return ex.ToString();
            
            if (value is Type || 
                value is ActionWrapper ||
                value is MemberInfo ||
                value is MethodBase ||
                value is Delegate)
                return null;
            
            if (value is Dictionary<string, object> dict)
            {
                var filtered = new Dictionary<string, object>();
                foreach (var kvp in dict)
                {
                    var filteredValue = FilterValue(kvp.Value);
                    if (filteredValue != null)
                        filtered[kvp.Key] = filteredValue;
                }

                return filtered.Count > 0 ? filtered : null;
            }
            
            if (value is System.Collections.IList list)
            {
                var filtered = new List<object?>();
                foreach (var item in list)
                {
                    var filteredItem = FilterValue(item);
                    if (filteredItem != null)
                        filtered.Add(filteredItem);
                }

                return filtered.Count > 0 ? filtered : null;
            }
            
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var filtered = new List<object?>();
                foreach (var item in enumerable)
                {
                    var filteredItem = FilterValue(item);
                    if (filteredItem != null)
                        filtered.Add(filteredItem);
                }

                return filtered.Count > 0 ? filtered : null;
            }
            
            try
            {
                System.Text.Json.JsonSerializer.Serialize(value);
                return value;
            }
            catch
            {
                return null;
            }
        }
    }

    public class ActionWrapperSaveModel
    {
        public string? ActionName { get; set; }
        public string DisplayName { get; set; }
        public string GUID { get; set; }

        public Dictionary<String, Object>? Arguments { get; set; }

        public ActionWrapper Wrap(IServiceProvider serviceProvider)
        {
            var result = new ActionWrapper(DisplayName, ActionName, new Arguments(Arguments ?? new Dictionary<string, object>()));
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
            Arguments = SessionData.FilterSerializableArguments(actionWrapper.Arguments.Source);
            GUID = actionWrapper.GUID;
        }
    }
}