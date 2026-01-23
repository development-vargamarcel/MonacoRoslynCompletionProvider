using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
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
            return HoverInformationProvider.Provide(Document, position, SemanticModel, cancellationToken);
        }

        public Task<TabCompletionResult[]> GetTabCompletion(int position, CancellationToken cancellationToken)
        {
            return TabCompletionProvider.Provide(Document, position, cancellationToken);
        }

        public Task<TabCompletionResult> GetCompletionResolve(int position, string suggestion, CancellationToken cancellationToken)
        {
            return TabCompletionProvider.ProvideDescription(Document, position, suggestion, cancellationToken);
        }

        public async Task<CodeCheckResult[]> GetCodeCheckResults(CancellationToken cancellationToken)
        {
            return await CodeCheckProvider.Provide(Diagnostics, Document, cancellationToken);
        }

        public Task<SignatureHelpResult> GetSignatureHelp(int position, CancellationToken cancellationToken)
        {
            return SignatureHelpProvider.Provide(Document, position, SemanticModel, cancellationToken);
        }

        public Task<CodeActionResult[]> GetCodeFormatting(CodeFormatRequest request, CancellationToken cancellationToken)
        {
            TextSpan? span = null;
            if (request.Start > 0 || request.End > 0)
            {
                span = TextSpan.FromBounds(request.Start, request.End);
            }
            return CodeFormatProvider.Provide(Document, span, cancellationToken);
        }

        public Task<GoToDefinitionResult> GetGoToDefinition(int position, CancellationToken cancellationToken)
        {
            return GoToDefinitionProvider.Provide(Document, position, cancellationToken);
        }

        public Task<CodeActionResult> GetRename(int position, string newName, CancellationToken cancellationToken)
        {
            return RenameProvider.Provide(Document, position, newName, cancellationToken);
        }
    }
}
