using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using MonacoRoslynCompletionProvider.Api;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public static class GoToDefinitionProvider
    {
        public static async Task<GoToDefinitionResult> Provide(Document document, int position, CancellationToken cancellationToken)
        {
            var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, position, cancellationToken);
            if (symbol == null) return null;

            var definition = await SymbolFinder.FindSourceDefinitionAsync(symbol, document.Project.Solution, cancellationToken);
            definition ??= symbol;

            var locations = definition.Locations;
            var resultLocations = new System.Collections.Generic.List<DefinitionLocation>();

            foreach (var loc in locations)
            {
                if (loc.IsInSource)
                {
                    resultLocations.Add(new DefinitionLocation
                    {
                        // We assume single file scenario for now or we return the same file.
                        // Ideally we should return the file path or a virtual URI.
                        // Since we are running in a stateless mode where we only know about "MyFile.cs",
                        // we check if it is in the current document.
                        Uri = loc.SourceTree.FilePath, // This will be "MyFile.cs"
                        OffsetFrom = loc.SourceSpan.Start,
                        OffsetTo = loc.SourceSpan.End
                    });
                }
            }

            return new GoToDefinitionResult
            {
                Definitions = resultLocations.ToArray()
            };
        }
    }
}
