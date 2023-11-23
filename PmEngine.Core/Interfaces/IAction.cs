namespace PmEngine.Core.Interfaces
{
	/// <summary>
	/// Интерфейс описывающий выполнение какого-либо действия пользователем.
	/// </summary>
	public interface IAction
	{
        /// <summary>
        /// Выполнить действие пользователя
        /// </summary>
        /// <param name="currentAction">Текущее действие пользователя</param>
        /// <param name="user">Сессия пользователя</param>
        /// <param name="arguments">Доп. параметры действия</param>
        /// <returns>Список следующих действий</returns>
        public Task<INextActionsMarkup?> DoAction(IActionWrapper currentAction, IUserSession user, IActionArguments arguments);

        /// <summary>
        /// Действие после выполнения экшена
        /// </summary>
        /// <param name="currentAction"></param>
        /// <param name="user"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Task AfterAction(IActionWrapper currentAction, IUserSession user, IActionArguments arguments)
        {
            return Task.CompletedTask;
        }
	}
}