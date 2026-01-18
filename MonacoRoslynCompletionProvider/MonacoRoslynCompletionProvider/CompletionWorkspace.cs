using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        // Static host services to avoid re-initializing MefHostServices which is expensive
        private static readonly Microsoft.CodeAnalysis.Host.HostServices _host;

        static CompletionWorkspace()
        {
            try
            {
                Assembly[] lst = new[] {
                    Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
                    Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
                    Assembly.Load("Microsoft.CodeAnalysis.Features"),
                    Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
                };

                _host = MefHostServices.Create(lst);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing HostServices: {ex}");
                throw;
            }
        }

        public CompletionWorkspace(string[] assemblies)
        {
            _workspace = new AdhocWorkspace(_host);

            var references = MetadataReferenceProvider.GetMetadataReferences();

            if (assemblies != null && assemblies.Length > 0)
            {
                for (int i = 0; i < assemblies.Length; i++)
                {
                    try
                    {
                        references.Add(MetadataReference.CreateFromFile(assemblies[i]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load reference {assemblies[i]}: {ex.Message}");
                    }
                }
            }

            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TempProject", "TempProject", LanguageNames.CSharp)
                .WithMetadataReferences(references)
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose))
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            _project = _workspace.AddProject(projectInfo);
        }

        public async Task<CompletionDocument> CreateDocument(string code, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
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

            using (var temp = new MemoryStream())
            {
                var result = compilation.Emit(temp);
                var semanticModel = compilation.GetSemanticModel(st, true);

                return new CompletionDocument(document, semanticModel, result);
            }
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
