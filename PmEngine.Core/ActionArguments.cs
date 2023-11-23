using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    /// <summary>
    /// Аргументы вызова действия
    /// </summary>
    public class ActionArguments : IActionArguments
    {
        public Dictionary<string, object> ToDict()
        {
            return Arguments;
        }

        /// <summary>
        /// Список аргументов в формате словаря. Все ключи автоматически преобразуются в lowcase
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; } = new();

        /// <summary>
        /// Установить значение аргумента
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void Set(string key, object? value)
        {
            var lowkey = key.ToLower();

            if (value is null)
            {
                if (Arguments.ContainsKey(lowkey))
                    Arguments.Remove(lowkey);
            }
            else
                Arguments[lowkey] = value;
        }

        /// <summary>
        /// Получение значения аргумента
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="key">Ключ</param>
        /// <returns>Значение</returns>
        public T? Get<T>(string key)
        {
            if(Arguments.TryGetValue(key, out object? firstVal))
                return (T?)firstVal;
            else if (Arguments.TryGetValue(key.ToLower(), out object? secondVal))
                return (T?)secondVal;
            else
            {
                var val = Arguments.FirstOrDefault(d => d.Key.ToLower() == key.ToLower()).Value;

                if (val is null)
                    return default;

                return (T?)val;
            }
        }

        /// <summary>
        /// Класс аргументов
        /// </summary>
        public ActionArguments()
        {
        }

        /// <summary>
        /// Аргументы с заполнением из словаря. Не ссылается на указанный словарь, а создает новый со значениями из указанного.
        /// </summary>
        /// <param name="args"></param>
        public ActionArguments(Dictionary<string, object> args)
        {
            Arguments = new();

            foreach (var arg in args)
                Arguments[arg.Key.ToLower()] = arg.Value;
        }
    }
}