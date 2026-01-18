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
    public class CompletionWorkspace
    {
        private Project _project;
        private AdhocWorkspace _workspace;

        private static readonly ConcurrentDictionary<string, CompletionWorkspace> _cache = new ConcurrentDictionary<string, CompletionWorkspace>();
        private static readonly Microsoft.CodeAnalysis.Host.HostServices _host;

        static CompletionWorkspace()
        {
            Assembly[] lst = new[] {
                Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
            };

            _host = MefHostServices.Create(lst);
        }

        public static CompletionWorkspace Create(params string[] assemblies)
        {
            var key = string.Join(";", assemblies?.OrderBy(x => x) ?? Enumerable.Empty<string>());
            return _cache.GetOrAdd(key, _ => CreateNew(assemblies));
        }

        private static CompletionWorkspace CreateNew(string[] assemblies)
        {
            var workspace = new AdhocWorkspace(_host);

            var references = MetadataReferenceProvider.DefaultMetadataReferences.ToList();

            if (assemblies != null && assemblies.Length > 0)
            {
                for (int i = 0; i < assemblies.Length; i++)
                {
                    references.Add(MetadataReference.CreateFromFile(assemblies[i]));
                }
            }

            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TempProject", "TempProject", LanguageNames.CSharp)
                .WithMetadataReferences(references)
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose))
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var project = workspace.AddProject(projectInfo);

            return new CompletionWorkspace() { _workspace = workspace, _project = project };
        }

        public async Task<CompletionDocument> CreateDocument(string code, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            var project = _workspace.CurrentSolution.GetProject(_project.Id);

            // Ensure compilation options match the requested output kind
            if (project.CompilationOptions.OutputKind != outputKind)
            {
                project = project.WithCompilationOptions(project.CompilationOptions.WithOutputKind(outputKind));
            }

            var document = project.AddDocument("MyFile.cs", SourceText.From(code));
            var compilation = await document.Project.GetCompilationAsync();
            var st = await document.GetSyntaxTreeAsync();

            using (var temp = new MemoryStream())
            {
                var result = compilation.Emit(temp);
                var semanticModel = compilation.GetSemanticModel(st, true);

                return new CompletionDocument(document, semanticModel, result);
            }
        }
    }
}
