namespace MonacoRoslynCompletionProvider.Api
{
    public class RenameRequest : IRequestWithPosition
    {
        public RenameRequest() { }

        public string Code { get; set; }
        public int Position { get; set; }
        public string NewName { get; set; }
        public string[] Assemblies { get; set; }
    }
}
