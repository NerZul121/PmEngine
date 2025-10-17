using System.Text.Json;
using System.Text.Json.Nodes;

namespace PmEngine.Core
{
    /// <summary>
    /// Action arguments.
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// Arguments source
        /// </summary>
        public Dictionary<string, object> Source { get; set; } = new(); 

        /// <summary>
        /// Асессоры для форматирования аргументов
        /// </summary>
        public List<IArgumentsAccessor> Accessors = new();

        /// <summary>
        /// Set argument value
        /// </summary>
        /// <param name="key">Key of argument</param>
        /// <param name="value">Value of argument</param>
        public void Set(string key, object? value)
        {
            var lowkey = key.ToLower();

            if (value is null)
            {
                if (Source.ContainsKey(lowkey))
                    Source.Remove(lowkey);
            }
            else
                Source[lowkey] = value;
        }

        /// <summary>
        /// Get argument value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public T? Get<T>(string key)
        {
            object? objValue;

            if (Source.TryGetValue(key, out object? firstVal))
                objValue = firstVal;
            else if (Source.TryGetValue(key.ToLower(), out object? secondVal))
                objValue = secondVal;
            else
            {
                var val = Source.FirstOrDefault(d => d.Key.ToLower() == key.ToLower()).Value;

                if (val is null)
                    objValue = default;
                else
                    objValue = val;
            }

            if (objValue is not null && objValue is JsonObject jsonValue)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(jsonValue.ToString());
                }
                catch { }

                try
                {
                    return jsonValue.GetValue<T>();
                }
                catch { }
            }

            if (objValue is not null && objValue is JsonElement jsonElement)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(jsonElement);
                }
                catch { }
            }

            if (objValue is null)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(objValue.ToString());
            }
            catch { }

            return (T?)objValue;
        }

        /// <summary>
        /// Casting this ActionArguments to another child type if need
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Cast<T>() where T : Arguments, new()
        {
            var result = new T();

            result.Set(Source);

            return result;
        }

        /// <summary>
        /// Set arguments
        /// </summary>
        /// <param name="args"></param>
        public void Set(Dictionary<string, object> args)
        {
            Source = args;
        }

        /// <summary>
        /// New empty action arguments
        /// </summary>
        public Arguments()
        {
        }

        /// <summary>
        /// New action arguments based on source
        /// </summary>
        /// <param name="args"></param>
        public Arguments(Dictionary<string, object> args)
        {
            Source = new();

            foreach (var arg in args)
                Source[arg.Key.ToLower()] = arg.Value;
        }

        #region Fields
        /// <summary>
        /// Input data from user input
        /// </summary>
        public string? InputData { get { return Get<string?>("inputData"); } set { Set("inputData", value); } }
        #endregion

        public T As<T>() where T : IArgumentsAccessor, new()
        {
            var accessor = Accessors.FirstOrDefault(a => a.GetType() == typeof(T));

            if (accessor is null)
            {
                accessor = new T();
                Accessors.Add(accessor);
            }

            accessor.Source = this;
            return (T)accessor;
        }
    }

    public interface IArgumentsAccessor
    {
        Arguments Source { get; set; }
    }

    public class ArgumentsAccessor : IArgumentsAccessor
    {
        public Arguments Source { get; set; }
    }
}