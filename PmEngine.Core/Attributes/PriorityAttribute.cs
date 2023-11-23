namespace PmEngine.Core.Attributes
{
	/// <summary>
	/// Аттрибут приоритетности для работы сервисов. <br/>
	/// Если встречается более одного сервиса, реализующего интрефейс, то будет выбран тот, у которого число Priority меньше (чем меньше число - тем выше приоритет)
	/// </summary>
	public class PriorityAttribute : Attribute
	{
		/// <summary>
		/// Значение приоритетности. Меньше - выше.
		/// </summary>
		public int Priority { get; set; }

        /// <summary>
        /// Аттрибут приоритетности для работы сервисов. <br/>
        /// Если встречается более одного сервиса, реализующего интрефейс, то будет выбран тот, у которого число Priority меньше (чем меньше число - тем выше приоритет)
        /// </summary>
        /// <param name="priority">Значение приоритетности. Меньше - выше.</param>
        public PriorityAttribute(int priority)
		{
            Priority = priority;
		}
	}
}