namespace PmEngine.Core.Entities
{
    /// <summary>
    /// User of this app
    /// </summary>
    public class UserEntity
    {
        public long Id { get; set; }

        /// <summary>
        /// Last online datetime
        /// </summary>
        public DateTime LastOnlineDate { get; set; }

        /// <summary>
        /// Registration date
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Кеш сесиии
        /// </summary>
        public string? SessionData { get; set; }

        public virtual List<UserPermissionEntity> Permissions { get; set; }

        public virtual List<UserLocalEntity> Locals { get; set; }
    }
}