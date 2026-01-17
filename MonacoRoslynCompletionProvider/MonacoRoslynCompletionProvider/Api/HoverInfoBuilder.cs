using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace MonacoRoslynCompletionProvider.Api
{
    public abstract class HoverInfoBuilder
    {
        private static readonly SymbolDisplayFormat DisplayFormat =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions:
                    SymbolDisplayMemberOptions.IncludeParameters |
                    SymbolDisplayMemberOptions.IncludeType |
                    SymbolDisplayMemberOptions.IncludeRef |
                    SymbolDisplayMemberOptions.IncludeModifiers |
                    SymbolDisplayMemberOptions.IncludeAccessibility,
                kindOptions:
                    SymbolDisplayKindOptions.None,
                parameterOptions:
                    SymbolDisplayParameterOptions.IncludeName |
                    SymbolDisplayParameterOptions.IncludeType |
                    SymbolDisplayParameterOptions.IncludeParamsRefOut |
                    SymbolDisplayParameterOptions.IncludeDefaultValue,
                localOptions: SymbolDisplayLocalOptions.IncludeType,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public static string Build(SymbolInfo symbolInfo)
        {
            if (symbolInfo.Symbol != null)
            {
                return Build(symbolInfo.Symbol);
            }
            return string.Empty;
        }

        public static string Build(ISymbol symbol)
        {
            var sb = new StringBuilder();

            sb.Append(symbol.ToDisplayString(DisplayFormat));

            var doc = GetDocumentation(symbol);
            if (!string.IsNullOrEmpty(doc))
            {
                sb.Append("\n").Append(doc);
            }
            return sb.ToString();
        }

        private static string GetDocumentation(ISymbol symbol)
        {
            var xml = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml)) return string.Empty;

            var sb = new StringBuilder();

            // Helper to strip tags and normalize whitespace
            string CleanText(string input)
            {
                 // Remove <c>, <code> tags but keep content?
                 // Or just remove all tags.
                 var noTags = Regex.Replace(input, @"<[^>]+>", " ");
                 return Regex.Replace(noTags.Trim(), @"\s+", " ");
            }

            // Extract Summary
            var match = Regex.Match(xml, @"<summary>(.*?)<\/summary>", RegexOptions.Singleline);
            if (match.Success)
            {
                sb.AppendLine(CleanText(match.Groups[1].Value));
            }

            // Extract Returns
             match = Regex.Match(xml, @"<returns>(.*?)<\/returns>", RegexOptions.Singleline);
            if (match.Success)
            {
                sb.AppendLine("Returns: " + CleanText(match.Groups[1].Value));
            }

            // Extract Remarks
             match = Regex.Match(xml, @"<remarks>(.*?)<\/remarks>", RegexOptions.Singleline);
            if (match.Success)
            {
                 sb.AppendLine("Remarks: " + CleanText(match.Groups[1].Value));
            }

            return sb.ToString().Trim();
        }
    }
}
