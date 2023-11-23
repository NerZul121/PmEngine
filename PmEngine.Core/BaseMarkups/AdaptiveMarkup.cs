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
        public IActionArguments Arguments { get; set; } = new ActionArguments();

        /// <summary>
        /// Базовая разметка
        /// </summary>
        public AdaptiveMarkup() { }

        /// <summary>
        /// Базовая разметка с указанием списка действий
        /// </summary>
        /// <param name="actions">Действия</param>
        public AdaptiveMarkup(IEnumerable<IActionWrapper> actions)
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
        public List<IActionWrapper> Actions { get; set; } = new();

        /// <summary>
        /// Получить действия
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<IActionWrapper>> GetNextActions()
        {
            if (Actions.Count > MinimumCount)
            {
                var newList = new List<List<IActionWrapper>>();
                var temp = new List<IActionWrapper>();
                foreach (var act in Actions)
                {
                    if (temp.Count == BlockSize)
                    {
                        newList.Add(temp);
                        temp = new List<IActionWrapper>();
                    }
                    temp.Add(act);
                }
                if (!newList.Contains(temp))
                    newList.Add(temp);

                return newList;
            }
            else
                return Actions.Select(a => new IActionWrapper[] { a });
        }

        /// <summary>
        /// Добавить действие
        /// </summary>
        /// <param name="action"></param>
        public void Add(IActionWrapper action)
        {
            Actions.Add(action);
        }

        /// <summary>
        /// Добавить действие
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
        /// <param name="arguments"></param>
        public IActionWrapper Add(string displayName, Type actionClass, IActionArguments? arguments = null)
        {
            var ar = arguments is null ? new ActionWrapper(displayName, actionClass) : new ActionWrapper(displayName, actionClass, arguments);
            Actions.Add(ar);
            return ar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IActionWrapper> GetFloatNextActions()
        {
            return Actions.ToArray();
        }
    }
}