namespace MonacoRoslynCompletionProvider.Api
{
    public class GoToDefinitionRequest : IRequestWithPosition
    {
        public GoToDefinitionRequest() { }

        public string Code { get; set; }
        public int Position { get; set; }
        public string[] Assemblies { get; set; }
    }
}
