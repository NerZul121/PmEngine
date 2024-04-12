namespace PmEngine.Core.Interfaces
{
	/// <summary>
	/// Some action
	/// </summary>
	public interface IAction
	{
        /// <summary>
        /// Execute action
        /// </summary>
        /// <param name="currentAction">this action data</param>
        /// <param name="user">user session</param>
        /// <returns>next available actions to user</returns>
        public Task<INextActionsMarkup?> DoAction(ActionWrapper currentAction, IUserSession user);

        /// <summary>
        /// After action
        /// </summary>
        /// <param name="currentAction">current action</param>
        /// <param name="user">user session</param>
        /// <returns></returns>
        public Task AfterAction(ActionWrapper currentAction, IUserSession user)
        {
            return Task.CompletedTask;
        }
	}
}