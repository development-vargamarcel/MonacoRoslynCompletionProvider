using System;

namespace MonacoRoslynCompletionProvider.Api
{
    public class CodeFormatRequest : IRequestWithCode
    {
        public string Code { get; set; }
        public string[] Assemblies { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
