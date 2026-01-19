using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MonacoRoslynCompletionProvider.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    /// <summary>
    /// Provides signature help (parameter info) for a given document and position.
    /// </summary>
    internal static class SignatureHelpProvider
    {
        public static async Task<SignatureHelpResult> Provide(Document document, int position, SemanticModel semanticModel)
        {
            var invocation = await InvocationContext.GetInvocation(document, position);
            if (invocation == null) return null;

            int activeParameter = 0;
            foreach (var comma in invocation.Separators)
            {
                if (comma.Span.Start > invocation.Position)
                    break;

                activeParameter += 1;
            }

            // Using a dictionary to keep track of unique signatures by label.
            var signaturesMap = new Dictionary<string, Signatures>();
            var bestScore = int.MinValue;
            Signatures bestScoredItem = null;

            var types = invocation.ArgumentTypes;
            ISymbol throughSymbol = null;
            ISymbol throughType = null;
            var methodGroup = invocation.SemanticModel.GetMemberGroup(invocation.Receiver).OfType<IMethodSymbol>();

            if (invocation.Receiver is MemberAccessExpressionSyntax memberAccess)
            {
                var throughExpression = memberAccess.Expression;
                var typeInfo = semanticModel.GetTypeInfo(throughExpression);
                throughSymbol = invocation.SemanticModel.GetSpeculativeSymbolInfo(invocation.Position, throughExpression, SpeculativeBindingOption.BindAsExpression).Symbol;
                throughType = invocation.SemanticModel.GetSpeculativeTypeInfo(invocation.Position, throughExpression, SpeculativeBindingOption.BindAsTypeOrNamespace).Type;

                var includeInstance = (throughSymbol != null && !(throughSymbol is ITypeSymbol)) ||
                    throughExpression is LiteralExpressionSyntax ||
                    throughExpression is TypeOfExpressionSyntax;

                var includeStatic = (throughSymbol is INamedTypeSymbol) || throughType != null;

                if (throughType == null)
                {
                    throughType = typeInfo.Type;
                    includeInstance = true;
                }
                methodGroup = methodGroup.Where(m => (m.IsStatic && includeStatic) || (!m.IsStatic && includeInstance));
            }
            else if (invocation.Receiver is SimpleNameSyntax && invocation.IsInStaticContext)
            {
                methodGroup = methodGroup.Where(m => m.IsStatic || m.MethodKind == MethodKind.LocalFunction);
            }

            foreach (var methodOverload in methodGroup)
            {
                var signature = BuildSignature(methodOverload);

                // If we don't have this signature yet, add it.
                if (!signaturesMap.ContainsKey(signature.Label))
                {
                    signaturesMap[signature.Label] = signature;
                }

                // Use the one stored in the map (canonical instance)
                var canonicalSignature = signaturesMap[signature.Label];

                var score = InvocationScore(methodOverload, types);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestScoredItem = canonicalSignature;
                }
            }

            var signaturesArray = signaturesMap.Values.ToArray();

            return new SignatureHelpResult()
            {
                Signatures = signaturesArray,
                ActiveParameter = activeParameter,
                ActiveSignature = bestScoredItem != null ? Array.IndexOf(signaturesArray, bestScoredItem) : 0
            };
        }

        private static Signatures BuildSignature(IMethodSymbol symbol)
        {
            var parameters = new List<Parameter>();
            foreach (var parameter in symbol.Parameters)
            {
                parameters.Add(new Parameter() { Label = parameter.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) });
            };
            var signature = new Signatures
            {
                Documentation = symbol.GetDocumentationCommentXml(),
                Label = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                Parameters = parameters.ToArray()
            };

            return signature;
        }

        private static int InvocationScore(IMethodSymbol symbol, IEnumerable<TypeInfo> types)
        {
            var parameters = symbol.Parameters;
            if (parameters.Count() < types.Count())
                return int.MinValue;

            var score = 0;
            var invocationEnum = types.GetEnumerator();
            var definitionEnum = parameters.GetEnumerator();
            while (invocationEnum.MoveNext() && definitionEnum.MoveNext())
            {
                if (invocationEnum.Current.ConvertedType == null)
                    score += 1;

                else if (SymbolEqualityComparer.Default.Equals(invocationEnum.Current.ConvertedType, definitionEnum.Current.Type))
                    score += 2;
            }
            return score;
        }
    }
}
