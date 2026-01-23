namespace MonacoRoslynCompletionProvider.Api
{
    public class GoToDefinitionResult
    {
        public string Uri { get; set; } // For simple cases, we might return null or a virtual URI.
        public DefinitionLocation[] Definitions { get; set; }
    }

    public class DefinitionLocation
    {
        public string Uri { get; set; }
        public int OffsetFrom { get; set; }
        public int OffsetTo { get; set; }
    }
}
