using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    internal class TabCompletionProvider
    {
        // Thanks to https://www.strathweb.com/2018/12/using-roslyn-c-completion-service-programmatically/
        public async Task<TabCompletionResult[]> Provide(Document document, int position)
        {
            var completionService = CompletionService.GetService(document);
            var results = await completionService.GetCompletionsAsync(document, position);

            var tabCompletionDTOs = new TabCompletionResult[results.ItemsList.Count];

            if (results != null)
            {
                var suggestions = new string[results.ItemsList.Count];

                for (int i = 0; i < results.ItemsList.Count; i++)
                {
                    var itemDescription = await completionService.GetDescriptionAsync(document, results.ItemsList[i]);

                    var dto = new TabCompletionResult();
                    dto.Suggestion = results.ItemsList[i].DisplayText;
                    dto.Description = itemDescription.Text;

                    tabCompletionDTOs[i] = dto;
                    suggestions[i] = results.ItemsList[i].DisplayText;
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
