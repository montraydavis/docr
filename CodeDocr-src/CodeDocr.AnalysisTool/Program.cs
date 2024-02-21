using CodeDocr.AnalysisTool.Analysis;
using CodeDocr.AnalysisTool.Helpers;
using CodeDocr.AnalysisTool.SourceDef;
using Cottle;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeDocr.AnalysisTool
{
    public class ModelTreePair
    {

        public SemanticModel Model { get; }
        public SyntaxTree Tree { get; }
        public ModelTreePair(SemanticModel model, SyntaxTree tree)
        {
            this.Model = model;
            this.Tree = tree;
        }
    }


    public partial class Program
    {
        public ISyntaxAnalysisService AnalysisService { get; }
        public VisualStudioInstance Instance { get; }
        public string SolutionPath { get; }

        public Program(VisualStudioInstance instance, string solutionPath, ISyntaxAnalysisService analysisService)
        {
            this.Instance = instance;
            this.SolutionPath = solutionPath;
            this.AnalysisService = analysisService;
        }


        private static ServiceProvider ConfigureServices(Action<IServiceCollection>? configure = null)
        {
            var servicesCollections = new ServiceCollection();

            servicesCollections.AddSingleton<ISyntaxAnalysisService, SyntaxAnalysisService>();
            servicesCollections.AddSingleton<ISyntaxHelper, SyntaxHelper>();

            configure?.Invoke(servicesCollections);

            var serviceProvider = servicesCollections.BuildServiceProvider();

            return serviceProvider;
        }


        static async Task Main(string[] args)
        {
            var solutionPath = args[0];

            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            var serviceProvider = ConfigureServices();

            var analysisService = serviceProvider.GetService<ISyntaxAnalysisService>();

            var program = new Program(instance, solutionPath, analysisService);

            await program.RunAsync();

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        }

        private async Task RunAsync()
        {
            MSBuildLocator.RegisterInstance(this.Instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                Console.WriteLine($"Loading solution '{this.SolutionPath}'");

                // Attach progress reporter so we print projects as they are loaded.
                var solution = await workspace.OpenSolutionAsync(this.SolutionPath, new ConsoleProgressReporter());
                Console.WriteLine($"Finished loading solution '{this.SolutionPath}'");

                var compilation = await solution.Projects
                    .Select(p => p.GetCompilationAsync())
                    .FirstOrDefault(c => c != null);

                if (compilation != null)
                {
                    // Get the semantic model for a specific document
                    var document = solution.Projects
                        .SelectMany(p => p.Documents)
                        .FirstOrDefault(d => d.FilePath.EndsWith(".cs"));

                    if (document != null)
                    {
                        var semanticModel = await document.GetSemanticModelAsync();

                        // Parse all solution source files
                        var sourceFiles = solution.Projects.SelectMany(p => p.Documents).Select(d => (Model: d.GetSemanticModelAsync().GetAwaiter().GetResult(), Path: d.FilePath)).Where(f => f.Path.EndsWith(".cs"));
                        var syntaxTrees = sourceFiles.Select(f => new ModelTreePair(f.Model, CSharpSyntaxTree.ParseText(File.ReadAllText(f.Path), path: f.Path)));

                        var syntaxTreeClasses = syntaxTrees;
                        var syntaxTreeWithClassesAndUsings = syntaxTrees.Select(syntax =>
                        {
                            var classes = this.AnalysisService.ParseClassDefinitions(syntax.Tree, syntax.Model);

                            return new
                            {
                                SyntaxTree = syntax,
                                Namespace = new NamespaceDefinition(syntax.Tree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault(), classes)
                            };
                        }).ToArray();

                        //var json = JsonConvert.SerializeObject(syntaxTreeWithClassesAndUsings);

                        //if (File.Exists("dump.json"))
                        //    File.Delete("dump.json");

                        //File.WriteAllText("dump.json", json);

                        var configuration = new DocumentConfiguration
                        {
                            NoOptimize = true,
                            Trimmer = DocumentConfiguration.TrimNothing

                        };
                        var templateUsings = File.ReadAllText("docs\\templates\\using.md");
                        var templateAttributes = File.ReadAllText("docs\\templates\\attribute.md");
                        var templateClass = File.ReadAllText("docs\\templates\\class.md");
                        var templateClassMethodParameters = File.ReadAllText("docs\\templates\\class-method-parameter.md");
                        var templateClassMethods = File.ReadAllText("docs\\templates\\class-method.md");
                        var templateClassProperties = File.ReadAllText("docs\\templates\\class-property.md");

                        var usingTemplateText = syntaxTreeWithClassesAndUsings
                            .Select(t =>
                            {
                                return t.Namespace.Classes
                                    .Select((c, idx) =>
                                    {
                                        var usingsList = c.Usings.Select(u =>
                                        {
                                            var documentResult = Cottle.Document.CreateDefault(templateUsings); // Create from template string
                                            var doc = documentResult.DocumentOrThrow; // Throws ParseException on error

                                            var context = Context.CreateBuiltin(new Dictionary<Value, Value>
                                            {
                                                ["namespace"] = u.Name // Declare new variable "who" with value "my friend"
                                            });

                                            return doc.Render(context);
                                        });

                                        var propertyList = c.Properties.Select(u =>
                                        {
                                            var documentResult = Cottle.Document.CreateDefault(templateClassProperties, configuration); // Create from template string
                                            var doc = documentResult.DocumentOrThrow; // Throws ParseException on error

                                            var context = Context.CreateBuiltin(new Dictionary<Value, Value>
                                            {
                                                ["propertyName"] = u.Name,
                                                ["propertyIsPublic"] = u.IsPublic.ToString(),
                                                ["propertyIsStatic"] = u.IsStatic.ToString(),
                                                ["propertyIsReadOnly"] = u.IsReadOnly.ToString(),
                                                ["propertyIsVirtual"] = u.IsVirtual.ToString(),
                                                ["propertyIsOverride"] = u.IsOverride.ToString(),
                                                ["propertyIsSealed"] = u.IsSealed.ToString(),
                                                ["propertyIsAbstract"] = u.IsAbstract.ToString(),
                                                ["propertyIsExtern"] = u.IsExtern.ToString(),
                                                ["propertyIsUnsafe"] = u.IsUnsafe.ToString(),
                                                ["propertyIsPartial"] = u.IsPartial.ToString(),
                                                ["propertyIsConst"] = u.IsConst.ToString(),
                                                ["propertyIsVolatile"] = u.IsVolatile.ToString(),
                                                ["propertyIsNew"] = u.IsNew.ToString(),
                                                ["propertyIsInternal"] = u.IsInternal.ToString(),
                                                ["propertyIsProtected"] = u.IsProtected.ToString(),
                                                ["propertyIsPrivate"] = u.IsPrivate.ToString()
                                            });

                                            var text = doc.Render(context);
                                            return text;
                                        }).ToArray();

                                        var methodList = c.Methods.Select(method =>
                                        {
                                            var documentResult = Cottle.Document.CreateDefault(templateClassMethods, configuration); // Create from template string
                                            var doc = documentResult.DocumentOrThrow; // Throws ParseException on error

                                            var parametersText = string.Empty;

                                            if (method.Parameters.Any())
                                                parametersText = ParseParametersText(method, configuration, templateClassMethodParameters);

                                            var context = Context.CreateBuiltin(new Dictionary<Value, Value>
                                            {
                                                ["methodName"] = method.Name,
                                                ["methodReturnType"] = method.ReturnType,
                                                ["methodIsPublic"] = method.IsPublic,
                                                ["methodIsStatic"] = method.IsStatic,
                                                ["methodIsVirtual"] = method.IsVirtual,
                                                ["methodIsOverride"] = method.IsOverride,
                                                ["methodIsAbstract"] = method.IsAbstract,
                                                ["methodIsExtern"] = method.IsExtern,
                                                ["methodIsInternal"] = method.IsInternal,
                                                ["methodIsProtected"] = method.IsProtected,
                                                ["methodIsPrivate"] = method.IsPrivate,
                                                ["methodDocumentation"] = method.Documentation.Comment,
                                                ["methodParameters"] = parametersText
                                            });

                                            var text = doc.Render(context);

                                            return text;
                                        });

                                        var usingText = (usingsList.Any() ? usingsList.Aggregate((a, b) => a + "\n" + b) : "");
                                        var propertyText = (propertyList.Any() ? propertyList.Aggregate((a, b) => a + "\n" + b) : "");
                                        var methodText = (methodList.Any() ? methodList.Aggregate((a, b) => a + "\n" + b) : "");

                                        var documentResult = Cottle.Document.CreateDefault(templateClass, configuration); // Create from template string
                                        var doc = documentResult.DocumentOrThrow; // Throws ParseException on error

                                        var context = Context.CreateBuiltin(new Dictionary<Value, Value>
                                        {
                                            ["className"] = c.Name,
                                            ["classUsings"] = usingText,
                                            ["classProperties"] = propertyText,
                                            ["classMethods"] = methodText
                                        });

                                        var text = doc.Render(context);
                                        return (Name: c.Name, Text: text);
                                    })
                                    .ToArray();
                            })
                            .SelectMany(c => c)
                            .ToArray();

                        if (Directory.Exists("docs/out") == false)
                        {
                            Directory.CreateDirectory("docs/out");
                        }
                        var li = usingTemplateText.ToList();

                        foreach (var template in li)
                        {
                            var index = li.IndexOf(template);

                            File.WriteAllText($"docs/out/{index}_{template.Name}.md", template.Text);
                        }
                    }
                }
            }
        }

        private static string ParseParametersText(MethodDefinition u, DocumentConfiguration configuration, string template)
        {
            return u.Parameters.Select(p =>
            {
                var documentResult = Cottle.Document.CreateDefault(template, configuration); // Create from template string
                var doc = documentResult.DocumentOrThrow; // Throws ParseException on error
                var context = Context.CreateBuiltin(new Dictionary<Value, Value>
                {
                    ["parameterName"] = p.Name,
                    ["parameterType"] = p.Type
                });

                var text = doc.Render(context);

                return text;
            }).Aggregate((a, b) => a + "\n" + b);
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }
    }
}
