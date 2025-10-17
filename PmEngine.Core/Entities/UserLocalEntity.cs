using Microsoft.EntityFrameworkCore;
using PmEngine.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PmEngine.Core.Entities
{
    /// <summary>
    /// Long lifetime variables of user.<br/>
    /// Use this for storage custom info out of user session lifetime
    /// </summary>
    [PrimaryKey("Name", "UserId")]
    public class UserLocalEntity : IDataEntity
    {
        /// <summary>
        /// Local key
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID of user owner
        /// </summary>
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity Owner { get; set; }

        /// <summary>
        /// Local value
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}