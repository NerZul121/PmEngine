namespace PmEngine.Core.Interfaces
{
    public interface IActionWrapper
    {
        public string GUID { get; }
        public INextActionsMarkup NextActions { get; set; }
        public string DisplayName { get; set; }
        public Type? ActionType { get; set; }
        public string? ActionTypeName { get; set; }
        public IActionArguments Arguments { get; set; }
        public bool Visible { get; set; }
        public string? ActionText { get; set; }
    }
}