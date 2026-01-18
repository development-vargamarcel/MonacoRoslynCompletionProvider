using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonacoRoslynCompletionProvider;
using MonacoRoslynCompletionProvider.Api;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Tests
{
    [TestClass]
    public class HoverBuilderTests
    {
        [TestMethod]
        public async Task HoverInfoBuilder_ShouldReturnMarkdown()
        {
            var code = @"
namespace TestNamespace {
    /// <summary>
    /// Describes a person.
    /// </summary>
    /// <param name=""name"">The name of the person.</param>
    public class Person(string name) {
    }
}";
            using var ws = new CompletionWorkspace(Array.Empty<string>());
            var document = await ws.CreateDocument(code);
            var syntaxTree = await document.Document.GetSyntaxTreeAsync();
            var semanticModel = document.SemanticModel;

            var classNode = syntaxTree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();
            var symbol = semanticModel.GetDeclaredSymbol(classNode);

            var info = HoverInfoBuilder.Build(symbol);

            // Verify Markdown structure
            Assert.IsTrue(info.Contains("```csharp"), "Should contain start of code block");
            Assert.IsTrue(info.Contains("TestNamespace.Person"), "Should contain class name in signature");
            Assert.IsTrue(info.Contains("```"), "Should contain end of code block");
            Assert.IsTrue(info.Contains("**Summary**"), "Should contain Summary header");
            Assert.IsTrue(info.Contains("Describes a person."), "Should contain summary text");
            // Parameters might not be in class symbol docs depending on how it's defined (primary constructor)
            // But let's check basic structure first.
        }

        [TestMethod]
        public async Task HoverInfoBuilder_MethodWithParams()
        {
             var code = @"
namespace TestNamespace {
    public class Test {
        /// <summary>
        /// Do something.
        /// </summary>
        /// <param name=""id"">The ID.</param>
        /// <returns>A boolean.</returns>
        public bool Do(int id) => true;
    }
}";
            using var ws = new CompletionWorkspace(Array.Empty<string>());
            var document = await ws.CreateDocument(code);
            var syntaxTree = await document.Document.GetSyntaxTreeAsync();
            var semanticModel = document.SemanticModel;

            var methodNode = syntaxTree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First();
            var symbol = semanticModel.GetDeclaredSymbol(methodNode);

            var info = HoverInfoBuilder.Build(symbol);

            Assert.IsTrue(info.Contains("**Parameters**"));
            Assert.IsTrue(info.Contains("- `id`: The ID."));
            Assert.IsTrue(info.Contains("**Returns**"));
            Assert.IsTrue(info.Contains("A boolean."));
        }
    }
}
