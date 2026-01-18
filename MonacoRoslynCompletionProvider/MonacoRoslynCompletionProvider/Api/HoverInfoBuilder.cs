using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;

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

            sb.AppendLine("```csharp");
            sb.AppendLine(symbol.ToDisplayString(DisplayFormat));
            sb.AppendLine("```");

            var doc = GetDocumentation(symbol);
            if (!string.IsNullOrEmpty(doc))
            {
                sb.Append(doc);
            }
            return sb.ToString();
        }

        private static string GetDocumentation(ISymbol symbol)
        {
            var xml = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml)) return string.Empty;

            try
            {
                var sb = new StringBuilder();
                // Wrap in root to ensure single root element
                var xdoc = XDocument.Parse("<root>" + xml + "</root>");

                string GetText(XElement element)
                {
                    if (element == null) return null;
                    return Regex.Replace(element.Value, @"\s+", " ").Trim();
                }

                var summary = GetText(xdoc.Descendants("summary").FirstOrDefault());
                if (!string.IsNullOrEmpty(summary))
                {
                    sb.AppendLine("**Summary**");
                    sb.AppendLine(summary);
                    sb.AppendLine();
                }

                var paramsElements = xdoc.Descendants("param");
                if (paramsElements.Any())
                {
                     sb.AppendLine("**Parameters**");
                     foreach(var param in paramsElements)
                     {
                         var name = param.Attribute("name")?.Value;
                         var desc = GetText(param);
                         if (!string.IsNullOrEmpty(name))
                         {
                             sb.AppendLine($"- `{name}`: {desc}");
                         }
                     }
                     sb.AppendLine();
                }

                var returns = GetText(xdoc.Descendants("returns").FirstOrDefault());
                if (!string.IsNullOrEmpty(returns))
                {
                    sb.AppendLine("**Returns**");
                    sb.AppendLine(returns);
                    sb.AppendLine();
                }

                var remarks = GetText(xdoc.Descendants("remarks").FirstOrDefault());
                if (!string.IsNullOrEmpty(remarks))
                {
                    sb.AppendLine("**Remarks**");
                    sb.AppendLine(remarks);
                    sb.AppendLine();
                }

                return sb.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
