using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using MonacoRoslynCompletionProvider.Api;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public class CompletionDocument
    {
        public Document Document { get; }
        public SemanticModel SemanticModel { get; }
        public EmitResult EmitResult { get; }

        internal CompletionDocument(Document document, SemanticModel semanticModel, EmitResult emitResult)
        {
            Document = document;
            SemanticModel = semanticModel;
            EmitResult = emitResult;
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
            return await CodeCheckProvider.Provide(EmitResult, Document, cancellationToken);
        }

        public Task<SignatureHelpResult> GetSignatureHelp(int position, CancellationToken cancellationToken)
        {
            return SignatureHelpProvider.Provide(Document, position, SemanticModel);
        }
    }
}
