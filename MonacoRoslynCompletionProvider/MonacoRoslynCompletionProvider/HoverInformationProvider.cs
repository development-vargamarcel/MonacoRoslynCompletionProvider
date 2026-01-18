using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    internal class HoverInformationProvider
    {
        public static async Task<HoverInfoResult> Provide(Document document, int position, SemanticModel semanticModel)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var token = syntaxRoot.FindToken(position);
            var expressionNode = token.Parent;

            ISymbol symbol = null;

            if (expressionNode is VariableDeclaratorSyntax variableDeclarator)
            {
                 symbol = semanticModel.GetDeclaredSymbol(variableDeclarator);
            }
            else if (expressionNode is PropertyDeclarationSyntax propertyDeclaration)
            {
                 symbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
            }
            else if (expressionNode is ParameterSyntax parameterSyntax)
            {
                 symbol = semanticModel.GetDeclaredSymbol(parameterSyntax);
            }
            else if (expressionNode is MethodDeclarationSyntax methodDeclaration)
            {
                 symbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            }
            else if (expressionNode is ClassDeclarationSyntax classDeclaration)
            {
                 symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            }
             else if (expressionNode is StructDeclarationSyntax structDeclaration)
            {
                 symbol = semanticModel.GetDeclaredSymbol(structDeclaration);
            }
             else if (expressionNode is InterfaceDeclarationSyntax interfaceDeclaration)
            {
                 symbol = semanticModel.GetDeclaredSymbol(interfaceDeclaration);
            }
             else if (expressionNode is EnumDeclarationSyntax enumDeclaration)
            {
                 symbol = semanticModel.GetDeclaredSymbol(enumDeclaration);
            }
            else
            {
                // Try GetSymbolInfo
                var symbolInfo = semanticModel.GetSymbolInfo(expressionNode);
                symbol = symbolInfo.Symbol;

                // If null, maybe it is a type? e.g. "Guid" in "Guid.NewGuid()"
                if (symbol == null && expressionNode is IdentifierNameSyntax)
                {
                     var typeInfo = semanticModel.GetTypeInfo(expressionNode);
                     symbol = typeInfo.Type;
                }
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
