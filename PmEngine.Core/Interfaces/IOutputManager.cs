namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Интерфейс, определяющий функционал вывода/отправки информации пользователю.
    /// </summary>
    public interface IOutputManager
    {
        /// <summary>
        /// Отправка нового контента
        /// </summary>
        /// <param name="content"></param>
        /// <param name="nextActions"></param>
        /// <param name="media"></param>
        /// <param name="additionals"></param>
        /// <returns></returns>
        public Task<int> ShowContent(string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, IActionArguments? additionals = null);

        /// <summary>
        /// Изменение существующего контента
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="content"></param>
        /// <param name="nextActions"></param>
        /// <param name="media"></param>
        /// <param name="additionals"></param>
        /// <returns></returns>
        public Task EditContent(int messageId, string content, INextActionsMarkup? nextActions = null, IEnumerable<object>? media = null, IActionArguments? additionals = null);

        /// <summary>
        /// Удаление сообщения
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public Task DeleteMessage(int messageId);
    }
}