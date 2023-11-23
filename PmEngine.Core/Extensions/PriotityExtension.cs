using PmEngine.Core.Attributes;

namespace PmEngine.Core.Extensions
{
    /// <summary>
    /// Расширение для приоритетности
    /// </summary>
    public static class PriorityExtension
    {
        /// <summary>
        /// Получение значения аттрибута Priority типа класса
        /// </summary>
        /// <param name="type">Тип класса</param>
        /// <param name="baseValue">Значение, если приоритет не указан (по умолчанию 0)</param>
        /// <returns>Приоритетность или 10</returns>
        public static int GetPriority(this Type type, int baseValue = 0)
        {
            var attr = type.GetCustomAttributes(typeof(PriorityAttribute), true).FirstOrDefault() as PriorityAttribute;
            return attr?.Priority ?? baseValue;
        }
    }
}