using PmEngine.Core.Attributes;
using System.Reflection;

namespace PmEngine.Core.Extensions
{
    /// <summary>
    /// Расширение для перечислений и Description
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Получение тега Descriprion из перечисления
        /// </summary>
        /// <param name="en"></param>
        /// <returns>Содержание тега Description</returns>
        public static string GetDescription(this Enum en)
        {
            var fieldInfo = en.GetType().GetField(en.ToString());

            if (fieldInfo is null)
                return "";

            var attribute = fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));

            if (attribute is null)
                return "";

            return ((DescriptionAttribute)attribute).Description;
        }

        /// <summary>
        /// Получение тега Descriprion из поля
        /// </summary>
        /// <param name="fieldInfo">Поле</param>
        /// <returns>Содержание тега Description</returns>
        public static string GetDescription(this FieldInfo fieldInfo)
        {
            var attribute = fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));

            if (attribute is null)
                return "";

            return ((DescriptionAttribute)attribute).Description;
        }


        /// <summary>
        /// Получение тега Description из свойства
        /// </summary>
        /// <param name="propertyInfo">свойство</param>
        /// <returns>Содержание тега Description</returns>
        public static string GetDescription(this PropertyInfo propertyInfo)
        {
            var attribute = propertyInfo.GetCustomAttribute(typeof(DescriptionAttribute));

            if (attribute is null)
                return "";

            return ((DescriptionAttribute)attribute).Description;
        }
    }
}