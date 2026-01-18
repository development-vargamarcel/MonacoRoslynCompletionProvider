using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonacoRoslynCompletionProvider;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MonacoRoslynCompletionProvider.Api;
using System;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public async Task HoverTest()
        {
            var code = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        /// <summary>
        /// This is the main method.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, world!"");
            Console.ReadLine();
        }
    }
}";
            var ws = CompletionWorkspace.Create();
            var document = await ws.CreateDocument(code);

            // Hover over "Main"
            // "static void Main(string[] args)" is at some position.
            // Let's find the position of "Main".
            int mainPos = code.IndexOf("Main");
            var info = await document.GetHoverInformation(mainPos, CancellationToken.None);

            Assert.IsNotNull(info, "Hover info for Main should not be null");
            Assert.IsTrue(info.Information.Contains("private static void Main"));

            // Hover over "Console"
            int consolePos = code.IndexOf("Console.");
            var info2 = await document.GetHoverInformation(consolePos, CancellationToken.None);

            Assert.IsNotNull(info2, "Hover info for Console should not be null");
            Assert.IsTrue(info2.Information.Contains("System.Console"));
        }

        [TestMethod]
        public async Task CompletionTest()
        {
             var code = @"using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.
        }
    }
}";
            int pos = code.IndexOf("Console.") + "Console.".Length;

            var request = new TabCompletionRequest(code, pos, new string[] {});
            var results = await CompletionRequestHandler.Handle(request);

            Assert.IsTrue(results.Length > 0);

            var writeLine = results.FirstOrDefault(r => r.Suggestion == "WriteLine");
            Assert.IsNotNull(writeLine);
            Assert.IsNotNull(writeLine.Tag);
            Assert.AreEqual("Method", writeLine.Tag);
        }

        [TestMethod]
        public async Task DocumentShouldNotContainErrorsWhenUsingTopLevelStatements()
        {
            const string code = @"using System;
Console.WriteLine(""Hello, world!"");
";

            var ws = CompletionWorkspace.Create();
            var document = await ws.CreateDocument(code, OutputKind.ConsoleApplication);
            var codeCheckResults = await document.GetCodeCheckResults(CancellationToken.None);

            Assert.IsTrue(codeCheckResults.All(r => r.Severity != CodeCheckSeverity.Error));
        }
    }
}
