using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    /// <summary>
    /// Provides tab completion results for a given document and position.
    /// </summary>
    internal class TabCompletionProvider
    {
        public static async Task<TabCompletionResult[]> Provide(Document document, int position, CancellationToken cancellationToken)
        {
            var completionService = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(document);
            if (completionService == null)
            {
                return Array.Empty<TabCompletionResult>();
            }

            var results = await completionService.GetCompletionsAsync(document, position, cancellationToken: cancellationToken);

            if (results == null)
            {
                return Array.Empty<TabCompletionResult>();
            }

            var items = results.ItemsList;

            // Map items without description
            return items.Select(item => new TabCompletionResult
            {
                Suggestion = item.DisplayText,
                Description = null, // Defer loading
                Tag = item.Tags.FirstOrDefault()
            }).ToArray();
        }

        public static async Task<TabCompletionResult> ProvideDescription(Document document, int position, string suggestion, CancellationToken cancellationToken)
        {
            var completionService = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(document);
            if (completionService == null) return null;

            var results = await completionService.GetCompletionsAsync(document, position, cancellationToken: cancellationToken);
            if (results == null) return null;

            var item = results.ItemsList.FirstOrDefault(i => i.DisplayText == suggestion);
            if (item == null) return null;

            var description = await completionService.GetDescriptionAsync(document, item, cancellationToken);

            return new TabCompletionResult
            {
                Suggestion = item.DisplayText,
                Description = description.Text,
                Tag = item.Tags.FirstOrDefault()
            };
        }
    }
}
