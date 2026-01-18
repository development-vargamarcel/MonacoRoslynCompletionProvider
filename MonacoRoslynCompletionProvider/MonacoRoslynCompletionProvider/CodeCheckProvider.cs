using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using MonacoRoslynCompletionProvider.Api;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public static class CodeCheckProvider
    {
        public static async Task<CodeCheckResult[]> Provide(EmitResult emitResult, Document document, CancellationToken cancellationToken)
        {
            var result = new List<CodeCheckResult>();
            var sourceText = await document.GetTextAsync(cancellationToken);

            foreach(var r in emitResult.Diagnostics)
            {
                var sev = r.Severity switch
                {
                    DiagnosticSeverity.Error => CodeCheckSeverity.Error,
                    DiagnosticSeverity.Warning => CodeCheckSeverity.Warning,
                    DiagnosticSeverity.Info => CodeCheckSeverity.Info,
                    _ => CodeCheckSeverity.Hint
                };

                var keyword = sourceText.GetSubText(r.Location.SourceSpan).ToString();

                var msg = new CodeCheckResult()
                {
                    Id = r.Id,
                    Keyword = keyword,
                    Message = r.GetMessage(),
                    OffsetFrom = r.Location.SourceSpan.Start,
                    OffsetTo = r.Location.SourceSpan.End,
                    Severity = sev,
                    SeverityNumeric = (int)sev
                };

                result.Add(msg);
            }
            return result.ToArray();
        }
    }
}
