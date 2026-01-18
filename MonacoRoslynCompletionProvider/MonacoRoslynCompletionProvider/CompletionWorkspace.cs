using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace MonacoRoslynCompletionProvider
{
    public class CompletionWorkspace
    {
        public static MetadataReference[] DefaultMetadataReferences = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(int).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                MetadataReference.CreateFromFile(typeof(DescriptionAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DataSet).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(XmlDocument).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(INotifyPropertyChanged).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location)
            };

        private Project _project;
        private AdhocWorkspace _workspace;
        private List<MetadataReference> _metadataReferences;

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

            var references = DefaultMetadataReferences.ToList();

            if (assemblies != null && assemblies.Length > 0)
            {
                for (int i = 0; i < assemblies.Length; i++)
                {
                    references.Add(MetadataReference.CreateFromFile(assemblies[i]));
                }
            }

            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TempProject", "TempProject", LanguageNames.CSharp)
                .WithMetadataReferences(references)
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose));
            var project = workspace.AddProject(projectInfo);


            return new CompletionWorkspace() { _workspace = workspace, _project = project, _metadataReferences = references };
        }

        public async Task<CompletionDocument> CreateDocument(string code, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            var project = _workspace.CurrentSolution.GetProject(_project.Id);
            var document = project.AddDocument("MyFile.cs", SourceText.From(code));
            var st = await document.GetSyntaxTreeAsync();
            var compilation =
            CSharpCompilation
                .Create("Temp",
                    new[] { st },
                    options: new CSharpCompilationOptions(outputKind),
                    references: _metadataReferences
                );

            using(var temp = new MemoryStream())
            {
                var result = compilation.Emit(temp);
                var semanticModel = compilation.GetSemanticModel(st, true);

                return new CompletionDocument(document, semanticModel, result);
            }
        }
    }
}
