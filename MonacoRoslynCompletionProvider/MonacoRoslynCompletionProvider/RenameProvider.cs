using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;
using MonacoRoslynCompletionProvider.Api;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public static class RenameProvider
    {
        public static async Task<CodeActionResult> Provide(Document document, int position, string newName, CancellationToken cancellationToken)
        {
            // Renamer.RenameSymbolAsync requires the whole solution to be valid and usually works best with workspace having proper solution structure.
            // Since we use AdhocWorkspace with a single project/document, it should work for symbols defined in source.

            var symbol = await Microsoft.CodeAnalysis.FindSymbols.SymbolFinder.FindSymbolAtPositionAsync(document, position, cancellationToken);
            if (symbol == null) return null;

            // Rename options
            var options = new SymbolRenameOptions();

            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, new SymbolRenameOptions(), newName, cancellationToken);

            var changes = new System.Collections.Generic.List<CodeActionChange>();

            foreach (var proj in newSolution.Projects)
            {
                foreach (var docId in proj.DocumentIds)
                {
                    var newDoc = proj.GetDocument(docId);
                    var oldDoc = document.Project.Solution.GetDocument(docId);

                    if (newDoc != null && oldDoc != null)
                    {
                        var textChanges = await newDoc.GetTextChangesAsync(oldDoc, cancellationToken);
                        foreach (var tc in textChanges)
                        {
                            changes.Add(new CodeActionChange
                            {
                                OffsetFrom = tc.Span.Start,
                                OffsetTo = tc.Span.End,
                                NewText = tc.NewText
                            });
                        }
                    }
                }
            }

            return new CodeActionResult
            {
                Title = $"Rename to {newName}",
                ChangesInDocument = changes.ToArray()
            };
        }
    }
}
