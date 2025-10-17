using PmEngine.Core.Attributes;
using System.ComponentModel;

namespace PmEngine.Core.Enums
{
    /// <summary>
    /// Тип операции с сообщением
    /// </summary>
    public enum MessageOperationType
    {
        /// <summary>
        /// Отправка
        /// </summary>
        [Description("Отправка")]
        Send,

        /// <summary>
        /// Обновление
        /// </summary>
        [Description("Обновление")]
        Update,

        /// <summary>
        /// Удаление
        /// </summary>
        [Description("Удаление")]
        Delete,

        /// <summary>
        /// Закрепление
        /// </summary>
        [Description("Закреп")]
        Pin,

        /// <summary>
        /// Открепить
        /// </summary>
        [Description("Откреп")]
        Unpin,

        /// <summary>
        /// Картинка
        /// </summary>
        [Description("Отправка картинки")]
        Photo,

        /// <summary>
        /// Картинка и текст
        /// </summary>
        [Description("Отправка картинки с текстом")]
        PhotoAndText,
    }
}