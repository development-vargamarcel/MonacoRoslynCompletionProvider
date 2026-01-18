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
            ValidateRequest(request);
            try
            {
                var document = await GetDocument(request, cancellationToken);
                return await document.GetTabCompletion(request.Position, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tab completion");
                throw;
            }
        }

        public async Task<HoverInfoResult> GetHoverInformation(HoverInfoRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);
            try
            {
                var document = await GetDocument(request, cancellationToken);
                return await document.GetHoverInformation(request.Position, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hover information");
                throw;
            }
        }

        public async Task<CodeCheckResult[]> GetCodeCheckResults(CodeCheckRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request, checkPosition: false);
            try
            {
                var document = await GetDocument(request, cancellationToken);
                return await document.GetCodeCheckResults(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting code check results");
                throw;
            }
        }

        public async Task<SignatureHelpResult> GetSignatureHelp(SignatureHelpRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);
            try
            {
                var document = await GetDocument(request, cancellationToken);
                return await document.GetSignatureHelp(request.Position, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting signature help");
                throw;
            }
        }

        private void ValidateRequest(IRequestWithCode request, bool checkPosition = true)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.Code))
            {
                // Warn but maybe allow? No, completion on empty string might be valid (e.g. empty file).
                // But request.Code should be the file content.
                // _logger.LogWarning("Request code is null or empty");
            }

            if (checkPosition && request is IRequestWithPosition posRequest)
            {
                 if (posRequest.Position < 0)
                 {
                     throw new ArgumentOutOfRangeException(nameof(posRequest.Position), "Position must be non-negative.");
                 }
            }
        }

        private async Task<CompletionDocument> GetDocument(IRequestWithCode request, CancellationToken cancellationToken)
        {
            var key = string.Join(";", request.Assemblies?.OrderBy(x => x) ?? Enumerable.Empty<string>());

            var workspace = _workspaceCache.GetOrAdd(key, _ => new CompletionWorkspace(request.Assemblies));

            return await workspace.CreateDocument(request.Code);
        }
    }
}
