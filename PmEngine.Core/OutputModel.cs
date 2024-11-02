using PmEngine.Core.Interfaces;

namespace PmEngine.Core
{
    public class OutputModel
    {
        public INextActionsMarkup NextActions { get; set; }
        public string? Text { get; set; }
        public object[]? Media { get; set; }
        public Arguments? Arguments { get; set; }
    }
}