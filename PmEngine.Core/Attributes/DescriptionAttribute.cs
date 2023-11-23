namespace PmEngine.Core.Attributes
{
	/// <summary>
	/// Аттрибут описания поля/перечисления
	/// </summary>
	public class DescriptionAttribute : Attribute
	{
		/// <summary>
		/// Текст описания
		/// </summary>
		public string Description { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        /// <param name="description">Текст описания</param>
        public DescriptionAttribute(string description)
		{
			Description = description;
		}
	}
}