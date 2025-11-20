using Microsoft.EntityFrameworkCore;

namespace PmEngine.Core.Entities
{
    [PrimaryKey(nameof(UserId), nameof(Permission))]
    public class UserPermissionEntity
    {
        public long UserId { get; set; }
        public virtual UserEntity User { get; set; }
        public string Permission { get; set; }
    }
}