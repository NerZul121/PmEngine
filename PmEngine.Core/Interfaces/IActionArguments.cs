namespace PmEngine.Core.Interfaces
{
    public interface IActionArguments
    {
        public string? InputData { get { return Get<string?>("inputData"); } set { Set("inputData", value); } }
        public T Get<T>(string key);
        public void Set(string key, object? value);
        public Dictionary<string, object> ToDict();
        public T Cast<T>(IActionArguments args) where T : IActionArguments, new();
    }
}