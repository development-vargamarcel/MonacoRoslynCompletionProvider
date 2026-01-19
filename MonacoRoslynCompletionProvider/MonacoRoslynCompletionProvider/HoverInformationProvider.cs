using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    /// <summary>
    /// Provides hover information for a given document and position.
    /// </summary>
    internal static class HoverInformationProvider
    {
        public static async Task<HoverInfoResult> Provide(Document document, int position, SemanticModel semanticModel)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var token = syntaxRoot.FindToken(position);
            var expressionNode = token.Parent;

            ISymbol symbol = null;

            switch (expressionNode)
            {
                case VariableDeclaratorSyntax variableDeclarator:
                    symbol = semanticModel.GetDeclaredSymbol(variableDeclarator);
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    symbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
                    break;
                case ParameterSyntax parameterSyntax:
                    symbol = semanticModel.GetDeclaredSymbol(parameterSyntax);
                    break;
                case MethodDeclarationSyntax methodDeclaration:
                    symbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
                    break;
                case ClassDeclarationSyntax classDeclaration:
                    symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                    break;
                case StructDeclarationSyntax structDeclaration:
                    symbol = semanticModel.GetDeclaredSymbol(structDeclaration);
                    break;
                case InterfaceDeclarationSyntax interfaceDeclaration:
                    symbol = semanticModel.GetDeclaredSymbol(interfaceDeclaration);
                    break;
                case EnumDeclarationSyntax enumDeclaration:
                    symbol = semanticModel.GetDeclaredSymbol(enumDeclaration);
                    break;
                default:
                    // Try GetSymbolInfo
                    var symbolInfo = semanticModel.GetSymbolInfo(expressionNode);
                    symbol = symbolInfo.Symbol;

                    // If null, maybe it is a type? e.g. "Guid" in "Guid.NewGuid()"
                    if (symbol == null && expressionNode is IdentifierNameSyntax)
                    {
                        var typeInfo = semanticModel.GetTypeInfo(expressionNode);
                        symbol = typeInfo.Type;
                    }
                    break;
            }

            if (symbol != null)
            {
                var location = expressionNode.GetLocation();
                var info = HoverInfoBuilder.Build(symbol);
                if (!string.IsNullOrEmpty(info))
                {
                    return new HoverInfoResult()
                    {
                        Information = info,
                        OffsetFrom = location.SourceSpan.Start,
                        OffsetTo = location.SourceSpan.End
                    };
                }
            }

            return null;
        }
    }
}
