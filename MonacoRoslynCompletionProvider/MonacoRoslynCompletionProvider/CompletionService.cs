using Microsoft.Extensions.Logging;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MonacoRoslynCompletionProvider
{
    public class CompletionService : ICompletionService, IDisposable
    {
        private readonly ILogger<CompletionService> _logger;
        // Cache workspaces to avoid expensive re-creation.
        // Note: For a real-world scenario with many different assembly combinations, use IMemoryCache with eviction.
        private readonly ConcurrentDictionary<string, CompletionWorkspace> _workspaceCache = new();

        public CompletionService(ILogger<CompletionService> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            foreach (var workspace in _workspaceCache.Values)
            {
                workspace.Dispose();
            }
            _workspaceCache.Clear();
        }

        public Task<TabCompletionResult[]> GetTabCompletion(TabCompletionRequest request, CancellationToken cancellationToken = default)
        {
            return ExecuteRequest(request, "TabCompletion", (doc, token) => doc.GetTabCompletion(request.Position, token),
                checkPosition: true, includeDiagnostics: false, cancellationToken);
        }

        public Task<TabCompletionResult> GetCompletionResolve(CompletionResolveRequest request, CancellationToken cancellationToken = default)
        {
            return ExecuteRequest(request, "CompletionResolve", (doc, token) => doc.GetCompletionResolve(request.Position, request.Suggestion, token),
                checkPosition: true, includeDiagnostics: false, cancellationToken);
        }

        public Task<HoverInfoResult> GetHoverInformation(HoverInfoRequest request, CancellationToken cancellationToken = default)
        {
            return ExecuteRequest(request, "HoverInformation", (doc, token) => doc.GetHoverInformation(request.Position, token),
                checkPosition: true, includeDiagnostics: false, cancellationToken);
        }

        public Task<CodeCheckResult[]> GetCodeCheckResults(CodeCheckRequest request, CancellationToken cancellationToken = default)
        {
            return ExecuteRequest(request, "CodeCheck", (doc, token) => doc.GetCodeCheckResults(token),
                checkPosition: false, includeDiagnostics: true, cancellationToken);
        }

        public Task<SignatureHelpResult> GetSignatureHelp(SignatureHelpRequest request, CancellationToken cancellationToken = default)
        {
            return ExecuteRequest(request, "SignatureHelp", (doc, token) => doc.GetSignatureHelp(request.Position, token),
                checkPosition: true, includeDiagnostics: false, cancellationToken);
        }

        private async Task<TResult> ExecuteRequest<TResult>(IRequestWithCode request, string operationName, Func<CompletionDocument, CancellationToken, Task<TResult>> action, bool checkPosition, bool includeDiagnostics, CancellationToken cancellationToken)
        {
            try
            {
                ValidateRequest(request, checkPosition);
                var document = await GetDocument(request, includeDiagnostics, cancellationToken);
                return await action(document, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing {OperationName}. Code length: {CodeLength}", operationName, request?.Code?.Length ?? 0);
                throw;
            }
        }

        private void ValidateRequest(IRequestWithCode request, bool checkPosition)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (checkPosition && request is IRequestWithPosition posRequest)
            {
                 if (posRequest.Position < 0)
                 {
                     throw new ArgumentOutOfRangeException(nameof(posRequest.Position), "Position must be non-negative.");
                 }
                 if (request.Code != null && posRequest.Position > request.Code.Length)
                 {
                      throw new ArgumentOutOfRangeException(nameof(posRequest.Position), "Position is outside the code bounds.");
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
