using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    /// <summary>
    /// Provides tab completion results for a given document and position.
    /// </summary>
    internal class TabCompletionProvider
    {
        public static async Task<TabCompletionResult[]> Provide(Document document, int position)
        {
            var completionService = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(document);
            var results = await completionService.GetCompletionsAsync(document, position);

            if (results == null)
            {
                return Array.Empty<TabCompletionResult>();
            }

            var items = results.ItemsList;

            var tasks = items.Select(async item =>
            {
                var description = await completionService.GetDescriptionAsync(document, item);
                return new TabCompletionResult
                {
                    Suggestion = item.DisplayText,
                    Description = description.Text,
                    Tag = item.Tags.FirstOrDefault()
                };
            });

            return await Task.WhenAll(tasks);
        }
    }
}
