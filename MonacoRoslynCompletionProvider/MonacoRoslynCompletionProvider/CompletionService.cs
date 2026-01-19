using Microsoft.Extensions.Logging;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MonacoRoslynCompletionProvider
{
    public class CompletionService : ICompletionService
    {
        private readonly ILogger<CompletionService> _logger;
        // Cache workspaces to avoid expensive re-creation
        private readonly ConcurrentDictionary<string, CompletionWorkspace> _workspaceCache = new ConcurrentDictionary<string, CompletionWorkspace>();

        public CompletionService(ILogger<CompletionService> logger)
        {
            _logger = logger;
        }

        public async Task<TabCompletionResult[]> GetTabCompletion(TabCompletionRequest request, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequest(request, "TabCompletion", async (doc, token) =>
                await doc.GetTabCompletion(request.Position, token),
                checkPosition: true, includeDiagnostics: false, cancellationToken);
        }

        public async Task<HoverInfoResult> GetHoverInformation(HoverInfoRequest request, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequest(request, "HoverInformation", async (doc, token) =>
                await doc.GetHoverInformation(request.Position, token),
                checkPosition: true, includeDiagnostics: false, cancellationToken);
        }

        public async Task<CodeCheckResult[]> GetCodeCheckResults(CodeCheckRequest request, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequest(request, "CodeCheck", async (doc, token) =>
                await doc.GetCodeCheckResults(token),
                checkPosition: false, includeDiagnostics: true, cancellationToken);
        }

        public async Task<SignatureHelpResult> GetSignatureHelp(SignatureHelpRequest request, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequest(request, "SignatureHelp", async (doc, token) =>
                await doc.GetSignatureHelp(request.Position, token),
                checkPosition: true, includeDiagnostics: false, cancellationToken);
        }

        private async Task<TResult> ExecuteRequest<TResult>(IRequestWithCode request, string operationName, Func<CompletionDocument, CancellationToken, Task<TResult>> action, bool checkPosition, bool includeDiagnostics, CancellationToken cancellationToken)
        {
            ValidateRequest(request, checkPosition);
            try
            {
                var document = await GetDocument(request, includeDiagnostics, cancellationToken);
                return await action(document, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing {OperationName}", operationName);
                throw;
            }
        }

        private void ValidateRequest(IRequestWithCode request, bool checkPosition = true)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // request.Code can be empty

            if (checkPosition && request is IRequestWithPosition posRequest)
            {
                 if (posRequest.Position < 0)
                 {
                     throw new ArgumentOutOfRangeException(nameof(posRequest.Position), "Position must be non-negative.");
                 }
            }
        }

        private async Task<CompletionDocument> GetDocument(IRequestWithCode request, bool includeDiagnostics, CancellationToken cancellationToken)
        {
            var key = string.Join(";", request.Assemblies?.OrderBy(x => x) ?? Enumerable.Empty<string>());

            var workspace = _workspaceCache.GetOrAdd(key, _ => new CompletionWorkspace(request.Assemblies, _logger));

            return await workspace.CreateDocument(request.Code, includeDiagnostics: includeDiagnostics);
        }
    }
}
