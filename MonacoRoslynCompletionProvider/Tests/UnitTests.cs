using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonacoRoslynCompletionProvider;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MonacoRoslynCompletionProvider.Api;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        private ICompletionService _completionService;

        [TestInitialize]
        public void Setup()
        {
            var logger = NullLogger<CompletionService>.Instance;
            _completionService = new CompletionService(logger);
        }

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
            // Using service directly to get hover info
            int mainPos = code.IndexOf("Main");
            var request = new HoverInfoRequest() { Code = code, Position = mainPos, Assemblies = Array.Empty<string>() };
            var info = await _completionService.GetHoverInformation(request);

            Assert.IsNotNull(info, "Hover info for Main should not be null");
            Assert.IsTrue(info.Information.Contains("private static void Main"));

            // Hover over "Console"
            int consolePos = code.IndexOf("Console.");
            var request2 = new HoverInfoRequest() { Code = code, Position = consolePos, Assemblies = Array.Empty<string>() };
            var info2 = await _completionService.GetHoverInformation(request2);

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

            var request = new TabCompletionRequest() { Code = code, Position = pos, Assemblies = Array.Empty<string>() };
            var results = await _completionService.GetTabCompletion(request);

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
            // Check manual creation with explicit console app
            using var ws = new CompletionWorkspace(Array.Empty<string>());
            var document = await ws.CreateDocument(code, OutputKind.ConsoleApplication, includeDiagnostics: true);
            var codeCheckResults = await document.GetCodeCheckResults(CancellationToken.None);

            Assert.IsTrue(codeCheckResults.All(r => r.Severity != CodeCheckSeverity.Error), "Should not have errors for valid top-level statements");
        }

        [TestMethod]
        public async Task CodeCheck_ShouldReturnErrors_ForInvalidCode()
        {
            const string code = @"using System;
class Program {
    void Main() {
        Console.WriteLine(""Missing semicolon"")
    }
}";
            var request = new CodeCheckRequest() { Code = code, Assemblies = Array.Empty<string>() };
            var results = await _completionService.GetCodeCheckResults(request);

            Assert.IsTrue(results.Any(r => r.Severity == CodeCheckSeverity.Error), "Should return syntax error");
            var error = results.First(r => r.Severity == CodeCheckSeverity.Error);
            Assert.IsTrue(error.Message.Contains(";"), "Error message should mention missing semicolon");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task ValidationTest_NegativePosition()
        {
             var request = new TabCompletionRequest() { Code = "class A {}", Position = -1 };
             await _completionService.GetTabCompletion(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidationTest_NullRequest()
        {
             await _completionService.GetTabCompletion(null);
        }
    }
}
