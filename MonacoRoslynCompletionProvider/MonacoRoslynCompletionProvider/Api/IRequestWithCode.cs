namespace MonacoRoslynCompletionProvider.Api
{
    public interface IRequestWithCode : IRequest
    {
        string Code { get; }
        string[] Assemblies { get; }
    }
}
