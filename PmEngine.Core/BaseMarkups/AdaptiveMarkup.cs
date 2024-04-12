using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseMarkups
{
    /// <summary>
    /// Адаптивная разметка, разбивающая действия на блоки по указанному кол-ву
    /// </summary>
    public class AdaptiveMarkup : INextActionsMarkup
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
        public AdaptiveMarkup() { }

        /// <summary>
        /// Базовая разметка с указанием списка действий
        /// </summary>
        /// <param name="actions">Действия</param>
        public AdaptiveMarkup(IEnumerable<ActionWrapper> actions)
        {
            Actions = actions.ToList();
        }

        /// <summary>
        /// Размер блока (сколько кнопок в строке)
        /// </summary>
        public int BlockSize { get; set; } = 2;

        /// <summary>
        /// Минимальное кол-во кнопок для активации разметки
        /// </summary>
        public int MinimumCount { get; set; } = 3;

        /// <summary>
        /// Действия
        /// </summary>
        public List<ActionWrapper> Actions { get; set; } = new();

        /// <summary>
        /// Получить действия
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<ActionWrapper>> GetNextActions()
        {
            if (Actions.Count > MinimumCount)
            {
                var newList = new List<List<ActionWrapper>>();
                var temp = new List<ActionWrapper>();
                foreach (var act in Actions)
                {
                    if (temp.Count == BlockSize)
                    {
                        newList.Add(temp);
                        temp = new List<ActionWrapper>();
                    }
                    temp.Add(act);
                }
                if (!newList.Contains(temp))
                    newList.Add(temp);

                return newList;
            }
            else
                return Actions.Select(a => new ActionWrapper[] { a });
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