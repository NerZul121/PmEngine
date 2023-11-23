namespace PmEngine.Core.Extensions
{
    /// <summary>
    /// Метод расширения для работы с Arguments в Dictionary
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// Получить значение/дефолт (null) из словаря по ключу. Key.ToLower() применяется автоматически!
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемого объекта</typeparam>
        /// <param name="dict">Словарь</param>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public static T? GetArgument<T>(this Dictionary<string, object> dict, string key)
        {
            if(dict.TryGetValue(key, out object? firstVal))
                return (T?)firstVal;
            else if (dict.TryGetValue(key.ToLower(), out object? secondVal))
                return (T?)secondVal;
            else
            {
                var val = dict.FirstOrDefault(d => d.Key.ToLower() == key.ToLower()).Value;

                if (val is null)
                    return default;

                return (T?)val;
            }
        }

        /// <summary>
        /// Добавить значение в аргументы
        /// </summary>
        /// <typeparam name="T">Тип добавляемого объекта</typeparam>
        /// <param name="dict">Словарь</param>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public static void AddArgument<T>(this Dictionary<string, object> dict, string key, T value)
        {
            dict.Add(key.ToLower(), value);
        }
    }
}
