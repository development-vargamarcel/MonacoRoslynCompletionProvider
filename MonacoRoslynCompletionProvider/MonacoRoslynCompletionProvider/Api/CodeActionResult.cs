namespace MonacoRoslynCompletionProvider.Api
{
    public class CodeActionResult
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public CodeActionChange[] ChangesInDocument { get; set; }
    }

    public class CodeActionChange
    {
        public int OffsetFrom { get; set; }
        public int OffsetTo { get; set; }
        public string NewText { get; set; }
    }
}
