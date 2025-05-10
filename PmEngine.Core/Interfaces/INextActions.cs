namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Интерфейс разметки вывода кнопок действий.<br/>
    /// Является оберткой над двумерным массивом, нужен для создания заготовок/макетов разметки, по которым будет отображаться инфомрация пользователю.
    /// </summary>
    public interface INextActionsMarkup
    {
        /// <summary>
        /// InLine мод клавиатуры
        /// </summary>
        public bool InLine { get; set; }

        /// <summary>
        /// Аргументы
        /// </summary>
        public Arguments Arguments { get; set; }

        /// <summary>
        /// Получение списка следующих действий в формате двумерного массива.<br/>
        /// Нужно для отрисовки кнопок в табличном формате
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<ActionWrapper>> GetNextActions();

        /// <summary>
        /// Получение списка следующих действий в форамте плоского одномерного массива
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ActionWrapper> GetFloatNextActions();

        /// <summary>
        /// Помечает все экшены, названия которых дублируются в формате "(№) DisplayName"
        /// </summary>
        /// <returns></returns>
        public INextActionsMarkup NumeredDuplicates()
        {
            if (!EngineProperties.NumerateDuplicates)
                return this;

            var actions = GetFloatNextActions();
            var options = actions.GroupBy(s => s.DisplayName);

            foreach (var group in options)
            {
                var groupListed = group.ToList();
                foreach (var item in group)
                {
                    if (item.DisplayName.EndsWith(" "))
                        continue;

                    var name = group.Count() > 1 ? $"({groupListed.IndexOf(item) + 1}) {item.DisplayName}" : item.DisplayName;
                    item.DisplayName = name;
                }
            }

            return this;
        }

        /// <summary>
        /// Add action to actions
        /// </summary>
        /// <param name="action"></param>
        public void Add(ActionWrapper action);

        /// <summary>
        /// Add action
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="actionClass"></param>
        /// <param name="arguments"></param>
        public ActionWrapper Add(string displayName, Type actionClass, Arguments? arguments = null);

        /// <summary>
        /// Add action
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="arguments"></param>
        public ActionWrapper Add<T>(string displayName, Arguments? arguments = null) where T : ActionWrapper;
    }
}
