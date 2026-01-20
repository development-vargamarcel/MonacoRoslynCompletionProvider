using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider.Logic
{
    public class InvocationContext
    {
        public static async Task<InvocationContext> GetInvocation(Document document, int position, CancellationToken cancellationToken = default)
        {
            var sourceText = await document.GetTextAsync(cancellationToken);
            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            var root = await tree.GetRootAsync(cancellationToken);
            var node = root.FindToken(position).Parent;

            while (node != null)
            {
                if (node is InvocationExpressionSyntax invocation && invocation.ArgumentList.Span.Contains(position))
                {
                    var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                    return new InvocationContext(semanticModel, position, invocation.Expression, invocation.ArgumentList, cancellationToken);
                }

                if (node is BaseObjectCreationExpressionSyntax objectCreation && (objectCreation.ArgumentList?.Span.Contains(position) ?? false))
                {
                    var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                    return new InvocationContext(semanticModel, position, objectCreation, objectCreation.ArgumentList, cancellationToken);
                }

                if (node is AttributeSyntax attributeSyntax && (attributeSyntax.ArgumentList?.Span.Contains(position) ?? false))
                {
                    var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                    return new InvocationContext(semanticModel, position, attributeSyntax, attributeSyntax.ArgumentList, cancellationToken);
                }

                node = node.Parent;
            }

            return null;
        }

        public SemanticModel SemanticModel { get; }
        public int Position { get; }
        public SyntaxNode Receiver { get; }
        public IEnumerable<TypeInfo> ArgumentTypes { get; }
        public IEnumerable<SyntaxToken> Separators { get; }
        public bool IsInStaticContext { get; }

        public InvocationContext(SemanticModel semModel, int position, SyntaxNode receiver, ArgumentListSyntax argList, CancellationToken cancellationToken)
        {
            SemanticModel = semModel;
            Position = position;
            Receiver = receiver;
            ArgumentTypes = argList.Arguments.Select(argument => semModel.GetTypeInfo(argument.Expression, cancellationToken));
            Separators = argList.Arguments.GetSeparators();
        }

        public InvocationContext(SemanticModel semModel, int position, SyntaxNode receiver, AttributeArgumentListSyntax argList, CancellationToken cancellationToken)
        {
            SemanticModel = semModel;
            Position = position;
            Receiver = receiver;
            ArgumentTypes = argList.Arguments.Select(argument => semModel.GetTypeInfo(argument.Expression, cancellationToken));
            Separators = argList.Arguments.GetSeparators();
        }
    }
}
