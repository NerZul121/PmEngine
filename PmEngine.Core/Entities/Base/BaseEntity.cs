using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Entities.Base
{
    /// <summary>
    /// BaseEntity
    /// </summary>
    public abstract class BaseEntity : IDataEntity
    {
		/// <summary>
		/// Id
		/// </summary>
		public long Id { get; set; }
	}
}