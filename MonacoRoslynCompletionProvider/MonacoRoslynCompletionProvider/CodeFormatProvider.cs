using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using MonacoRoslynCompletionProvider.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public static class CodeFormatProvider
    {
        public static async Task<CodeActionResult[]> Provide(Document document, TextSpan? span, CancellationToken cancellationToken)
        {
            var formattedDocument = span.HasValue
                ? await Formatter.FormatAsync(document, span.Value, cancellationToken: cancellationToken)
                : await Formatter.FormatAsync(document, cancellationToken: cancellationToken);

            var changes = await formattedDocument.GetTextChangesAsync(document, cancellationToken);

            var result = new CodeActionResult
            {
                Title = "Format Document",
                ChangesInDocument = changes.Select(c => new CodeActionChange
                {
                    OffsetFrom = c.Span.Start,
                    OffsetTo = c.Span.End,
                    NewText = c.NewText
                }).ToArray()
            };

            return new[] { result };
        }
    }
}
