using MonacoRoslynCompletionProvider.Api;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public interface ICompletionService
    {
        Task<TabCompletionResult[]> GetTabCompletion(TabCompletionRequest request, CancellationToken cancellationToken = default);
        Task<TabCompletionResult> GetCompletionResolve(CompletionResolveRequest request, CancellationToken cancellationToken = default);
        Task<HoverInfoResult> GetHoverInformation(HoverInfoRequest request, CancellationToken cancellationToken = default);
        Task<CodeCheckResult[]> GetCodeCheckResults(CodeCheckRequest request, CancellationToken cancellationToken = default);
        Task<SignatureHelpResult> GetSignatureHelp(SignatureHelpRequest request, CancellationToken cancellationToken = default);
        Task<CodeActionResult[]> GetCodeFormatting(CodeFormatRequest request, CancellationToken cancellationToken = default);
        Task<GoToDefinitionResult> GetGoToDefinition(GoToDefinitionRequest request, CancellationToken cancellationToken = default);
        Task<CodeActionResult> GetRename(RenameRequest request, CancellationToken cancellationToken = default);
    }
}
