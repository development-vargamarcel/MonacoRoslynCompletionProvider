using MonacoRoslynCompletionProvider.Api;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public static class CompletionRequestHandler
    {
        public async static Task<TabCompletionResult[]> Handle(TabCompletionRequest tabCompletionRequest)
        {
            var document = await GetDocument(tabCompletionRequest);
            return await document.GetTabCompletion(tabCompletionRequest.Position, CancellationToken.None);
        }

        public async static Task<HoverInfoResult> Handle(HoverInfoRequest hoverInfoRequest)
        {
            var document = await GetDocument(hoverInfoRequest);
            return await document.GetHoverInformation(hoverInfoRequest.Position, CancellationToken.None);
        }

        public async static Task<CodeCheckResult[]> Handle(CodeCheckRequest codeCheckRequest)
        {
            var document = await GetDocument(codeCheckRequest);
            return await document.GetCodeCheckResults(CancellationToken.None);
        }

        public async static Task<SignatureHelpResult> Handle(SignatureHelpRequest signatureHelpRequest)
        {
            var document = await GetDocument(signatureHelpRequest);
            return await document.GetSignatureHelp(signatureHelpRequest.Position, CancellationToken.None);
        }

        private async static Task<CompletionDocument> GetDocument(IRequestWithCode request)
        {
            var workspace = CompletionWorkspace.Create(request.Assemblies);
            return await workspace.CreateDocument(request.Code);
        }
    }
}
