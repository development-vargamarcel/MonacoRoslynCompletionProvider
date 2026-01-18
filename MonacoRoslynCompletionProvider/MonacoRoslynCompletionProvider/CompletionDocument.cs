using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using MonacoRoslynCompletionProvider.Api;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public class CompletionDocument
    {
        public Document Document { get; }
        public SemanticModel SemanticModel { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        internal CompletionDocument(Document document, SemanticModel semanticModel, ImmutableArray<Diagnostic> diagnostics)
        {
            Document = document;
            SemanticModel = semanticModel;
            Diagnostics = diagnostics;
        }

        public Task<HoverInfoResult> GetHoverInformation(int position, CancellationToken cancellationToken)
        {
            return HoverInformationProvider.Provide(Document, position, SemanticModel);
        }

        public Task<TabCompletionResult[]> GetTabCompletion(int position, CancellationToken cancellationToken)
        {
            return TabCompletionProvider.Provide(Document, position);
        }

        public async Task<CodeCheckResult[]> GetCodeCheckResults(CancellationToken cancellationToken)
        {
            return await CodeCheckProvider.Provide(Diagnostics, Document, cancellationToken);
        }

        public Task<SignatureHelpResult> GetSignatureHelp(int position, CancellationToken cancellationToken)
        {
            return SignatureHelpProvider.Provide(Document, position, SemanticModel);
        }
    }
}
