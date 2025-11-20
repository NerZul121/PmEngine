using PmEngine.Core.SessionElements;

namespace PmEngine.Core.Interfaces
{
    /// <summary>
    /// Обработчик замены значений в тексте перед его отправкой
    /// </summary>
    public interface IOutputMutation
    {
        /// <summary>
        /// Обработка текста перед его отправкой
        /// </summary>
        /// <param name="content">Исходная строка</param>
        /// <param name="user">Пользователь</param>
        /// <returns>Отредактированная строка</returns>
        public Task<string> Mutate(string content, UserSession user);
    }
}