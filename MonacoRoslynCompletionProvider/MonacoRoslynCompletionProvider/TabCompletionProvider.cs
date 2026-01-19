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
        // Thanks to https://www.strathweb.com/2018/12/using-roslyn-c-completion-service-programmatically/
        public static async Task<TabCompletionResult[]> Provide(Document document, int position)
        {
            var completionService = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(document);
            var results = await completionService.GetCompletionsAsync(document, position);

            if (results != null)
            {
                var tabCompletionDTOs = new TabCompletionResult[results.ItemsList.Count];

                // Note: itemsList is used because results.Items is obsolete.
                for (int i = 0; i < results.ItemsList.Count; i++)
                {
                    var item = results.ItemsList[i];
                    var itemDescription = await completionService.GetDescriptionAsync(document, item);

                    var dto = new TabCompletionResult();
                    dto.Suggestion = item.DisplayText;
                    dto.Description = itemDescription.Text;

                    if (item.Tags != null && item.Tags.Length > 0)
                    {
                         dto.Tag = item.Tags[0];
                    }

                    tabCompletionDTOs[i] = dto;
                }

                return tabCompletionDTOs;
            }
            else
            {
                return Array.Empty<TabCompletionResult>();
            }
        }
    }
}
