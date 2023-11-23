namespace PmEngine.Core.Enums
{
	/// <summary>
	/// Type of user right
	/// </summary>
	public enum UserType
	{
		/// <summary>
		/// Banned user
		/// </summary>
		Banned = -1,
		/// <summary>
		/// Simple user
		/// </summary>
		Standart,
		/// <summary>
		/// Moderator of bot
		/// </summary>
		Moderator,
		/// <summary>
		/// Admin/Owner of bot
		/// </summary>
		Admin,
		/// <summary>
		/// Technical user
		/// </summary>
		Techincal
	}
}