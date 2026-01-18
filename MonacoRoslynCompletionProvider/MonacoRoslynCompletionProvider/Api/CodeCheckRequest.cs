namespace MonacoRoslynCompletionProvider.Api
{
    public class CodeCheckRequest : IRequestWithCode
    {
        public CodeCheckRequest()
        { }

        public CodeCheckRequest(string code, string[] assemblies)
        {
            this.Code = code;
            this.Assemblies = assemblies;
        }

        public virtual string Code { get; set; }

        public virtual string[] Assemblies { get; set; }
    }
}
