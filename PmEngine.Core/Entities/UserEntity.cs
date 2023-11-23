using PmEngine.Core.Entities.Base;

namespace PmEngine.Core.Entities
{
    /// <summary>
    /// User of this app
    /// </summary>
    public class UserEntity : BaseEntity
    {
        /// <summary>
        /// Type of user
        /// </summary>
        public int UserType { get; set; } = (int)Enums.UserType.Standart;

        /// <summary>
        /// Last online datetime
        /// </summary>
        public DateTime LastOnlineDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Registration date
        /// </summary>
        public DateTime RegistrationDate {  get; set; } = DateTime.Now;

        public string? SessionData { get; set; }

        /// <summary>
        /// User
        /// </summary>
        public UserEntity()
        {
        }
    }
}