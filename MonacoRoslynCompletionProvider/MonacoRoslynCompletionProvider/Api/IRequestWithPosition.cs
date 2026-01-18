namespace MonacoRoslynCompletionProvider.Api
{
    public interface IRequestWithPosition : IRequestWithCode
    {
        int Position { get; }
    }
}
