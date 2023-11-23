using System.Reflection;

namespace PmEngine.Core.Localization
{
    /// <summary>
    /// Класс, помогающий реализовать функцию локализации в приложении.<br/>
    /// Работает с аттрибутом Localzation, в котором должны быть указаны язык и текст на этом языке
    /// </summary>
    public static class Localization
    {
        /// <summary>
        /// Получить локализацию из аттрибута
        /// </summary>
        /// <param name="text"></param>
        /// <param name="localization"></param>
        /// <returns></returns>
        public static string Localize(this Enum text, string localization)
        {
            var fieldInfo = text.GetType().GetField(text.ToString());

            if (fieldInfo is null)
                return "NaN-1";

            var attributes = fieldInfo.GetCustomAttributes(typeof(LocalizationAttribute)).Select(a => (LocalizationAttribute)a);

            if (attributes is null || !attributes.Any())
                return "NaN-2";

            var attr = attributes.FirstOrDefault(a => a.Lang == localization);

            return attr is null ? attributes.First().Text : attr.Text;
        }
    }

    /// <summary>
    /// Аттрибут локализации.<br/>
    /// Для использования необходимо присвоить этот аттрибут элементам Enum, обозначив язык и текст на этом языке. Например:
    /// <code>
    /// public enum Texts
    /// {
    ///     [Localization("RU", "Привет!")]
    ///     [Localization("EN", "Hello!")]
    ///     HelloWorld
    /// }
    /// 
    /// Console.WriteLine(Texts.Hello.Localize("RU"));
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class LocalizationAttribute : Attribute
    {
        /// <summary>
        /// Язык (Напр. "RU")
        /// </summary>
        public string Lang {get; set; }

        /// <summary>
        /// Локализированный текст
        /// </summary>
        public string Text { get; set;}

        /// <summary>
        /// Локализация
        /// </summary>
        /// <param name="lcs">Язык</param>
        /// <param name="text">Текст</param>
        public LocalizationAttribute(string lcs, string text)
        {
            Lang = lcs;
            Text = text;
        }
    }
}
