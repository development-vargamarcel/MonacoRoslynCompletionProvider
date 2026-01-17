using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace MonacoRoslynCompletionProvider.Api
{
    public abstract class HoverInfoBuilder
    {
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

            if (symbol is IMethodSymbol methodsymbol)
            {
                sb.Append("(method) ").Append(methodsymbol.DeclaredAccessibility.ToString().ToLower()).Append(' ');
                if (methodsymbol.IsStatic)
                    sb.Append("static").Append(' ');
                sb.Append(methodsymbol.Name).Append('(');
                for (var i = 0; i < methodsymbol.Parameters.Length; i++)
                {
                    sb.Append(methodsymbol.Parameters[i].Type).Append(' ').Append(methodsymbol.Parameters[i].Name);
                    if (i < (methodsymbol.Parameters.Length - 1)) sb.Append(", ");
                }
                sb.Append(") : ");
                sb.Append(methodsymbol.ReturnType);
            }
            else if (symbol is ILocalSymbol localsymbol)
            {
                sb.Append("(local) ").Append(localsymbol.Name).Append(" : ");
                if (localsymbol.IsConst)
                    sb.Append("const").Append(' ');
                sb.Append(localsymbol.Type);
            }
            else if (symbol is IFieldSymbol fieldSymbol)
            {
                sb.Append("(field) ").Append(fieldSymbol.Name).Append(" : ").Append(fieldSymbol.DeclaredAccessibility.ToString().ToLower()).Append(' ');
                if (fieldSymbol.IsStatic)
                    sb.Append("static").Append(' ');
                if (fieldSymbol.IsReadOnly)
                    sb.Append("readonly").Append(' ');
                if (fieldSymbol.IsConst)
                    sb.Append("const").Append(' ');
                sb.Append(fieldSymbol.Type);
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                sb.Append("(property) ").Append(propertySymbol.Name).Append(" : ").Append(propertySymbol.DeclaredAccessibility.ToString().ToLower()).Append(' ');
                if (propertySymbol.IsStatic)
                    sb.Append("static").Append(' ');
                if (propertySymbol.IsReadOnly)
                    sb.Append("readonly").Append(' ');
                sb.Append(propertySymbol.Type);

                sb.Append(" { ");
                if (propertySymbol.GetMethod != null) sb.Append("get; ");
                if (propertySymbol.SetMethod != null) sb.Append("set; ");
                sb.Append("}");
            }
            else if (symbol is INamedTypeSymbol typeSymbol)
            {
                switch (typeSymbol.TypeKind)
                {
                    case TypeKind.Class:
                        sb.Append("(class) ");
                        break;
                    case TypeKind.Struct:
                        sb.Append("(struct) ");
                        break;
                    case TypeKind.Interface:
                        sb.Append("(interface) ");
                        break;
                    case TypeKind.Enum:
                        sb.Append("(enum) ");
                        break;
                    case TypeKind.Delegate:
                        sb.Append("(delegate) ");
                        break;
                    default:
                        sb.Append("(type) ");
                        break;
                }

                sb.Append(typeSymbol.DeclaredAccessibility.ToString().ToLower()).Append(' ');
                if (typeSymbol.IsStatic)
                    sb.Append("static").Append(' ');
                if (typeSymbol.IsAbstract && typeSymbol.TypeKind == TypeKind.Class)
                     sb.Append("abstract").Append(' ');
                if (typeSymbol.IsSealed && typeSymbol.TypeKind == TypeKind.Class)
                     sb.Append("sealed").Append(' ');

                sb.Append(typeSymbol.ToDisplayString());
            }
            else if (symbol is IParameterSymbol parameterSymbol)
            {
                sb.Append("(parameter) ").Append(parameterSymbol.Type).Append(' ').Append(parameterSymbol.Name);
            }

            if (sb.Length > 0)
            {
                var doc = GetDocumentation(symbol);
                if (!string.IsNullOrEmpty(doc))
                {
                    sb.Append("\n").Append(doc);
                }
                return sb.ToString();
            }

            // Fallback for other symbols
            return symbol.ToString();
        }

        private static string GetDocumentation(ISymbol symbol)
        {
            var xml = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml)) return string.Empty;

            var match = Regex.Match(xml, @"<summary>(.*?)<\/summary>", RegexOptions.Singleline);
            if (match.Success)
            {
                return Regex.Replace(match.Groups[1].Value.Trim(), @"\s+", " ");
            }
            return string.Empty;
        }
    }
}
