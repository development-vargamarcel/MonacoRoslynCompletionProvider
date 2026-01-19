using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MonacoRoslynCompletionProvider
{
    public class CompletionWorkspace : IDisposable
    {
        private readonly Project _project;
        private readonly AdhocWorkspace _workspace;
        private bool _disposed;
        private readonly ILogger _logger;

        // Static host services to avoid re-initializing MefHostServices which is expensive
        private static readonly Microsoft.CodeAnalysis.Host.HostServices _host;
        private static readonly Exception _hostInitException;

        static CompletionWorkspace()
        {
            try
            {
                var assemblyNames = new[]
                {
                    "Microsoft.CodeAnalysis.Workspaces",
                    "Microsoft.CodeAnalysis.CSharp.Workspaces",
                    "Microsoft.CodeAnalysis.Features",
                    "Microsoft.CodeAnalysis.CSharp.Features"
                };

                var assemblies = assemblyNames
                    .Select(name => Assembly.Load(name))
                    .ToArray();

                _host = MefHostServices.Create(assemblies);
            }
            catch (Exception ex)
            {
                _hostInitException = ex;
            }
        }

        public CompletionWorkspace(string[] assemblies, ILogger logger = null)
        {
            _logger = logger;
            if (_hostInitException != null)
            {
                _logger?.LogError(_hostInitException, "HostServices initialization failed");
                throw new InvalidOperationException("HostServices initialization failed", _hostInitException);
            }

            _workspace = new AdhocWorkspace(_host);

            var references = MetadataReferenceProvider.GetMetadataReferences();
            AddAdditionalReferences(assemblies, references);

            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TempProject", "TempProject", LanguageNames.CSharp)
                .WithMetadataReferences(references)
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose))
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            _project = _workspace.AddProject(projectInfo);
        }

        private void AddAdditionalReferences(string[] assemblies, List<MetadataReference> references)
        {
            if (assemblies == null || assemblies.Length == 0) return;

            foreach (var asm in assemblies)
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(asm));
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to load reference {Assembly}", asm);
                }
            }
        }

        public async Task<CompletionDocument> CreateDocument(string code, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, bool includeDiagnostics = false)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CompletionWorkspace));

            var project = _workspace.CurrentSolution.GetProject(_project.Id);

            // Ensure compilation options match the requested output kind
            if (project.CompilationOptions.OutputKind != outputKind)
            {
                project = project.WithCompilationOptions(project.CompilationOptions.WithOutputKind(outputKind));
            }

            var document = project.AddDocument("MyFile.cs", SourceText.From(code ?? string.Empty));
            var compilation = await document.Project.GetCompilationAsync();
            var st = await document.GetSyntaxTreeAsync();
            var semanticModel = compilation.GetSemanticModel(st, true);

            var diagnostics = includeDiagnostics ? compilation.GetDiagnostics() : ImmutableArray<Diagnostic>.Empty;

            return new CompletionDocument(document, semanticModel, diagnostics);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _workspace.Dispose();
                _disposed = true;
            }
        }
    }
}
