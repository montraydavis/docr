using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace CodeDocr.Semantics
{

    public partial class Program
    {
        public VisualStudioInstance Instance { get; }
        public string SolutionPath { get; }

        public Program(VisualStudioInstance instance, string solutionPath)
        {
            this.Instance = instance;
            this.SolutionPath = solutionPath;
        }


        /// <summary>
        /// Configure the services for the application.
        /// </summary>
        private static ServiceProvider ConfigureServices(Action<IServiceCollection>? configure = null)
        {
            var servicesCollections = new ServiceCollection();



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

            var program = new Program(instance, solutionPath);

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
                    
                var compilation = solution.Projects
                    .Select(p => p.GetCompilationAsync().GetAwaiter().GetResult())
                    .Where(p => p != null)
                    .FirstOrDefault(c => c != null);

                if (compilation != null)
                {
                    // Get the semantic model for a specific document
                    var documents = solution.Projects
                        .SelectMany(p => p.Documents)
                        .Where(d => d.FilePath?.EndsWith(".cs") == true);

                    foreach (var document in documents)
                    {
                        var syntaxTree = await document.GetSyntaxTreeAsync();

                        if (syntaxTree != null)
                        {
                            var rootNode = await syntaxTree.GetRootAsync();

                            var docrSemantics = new DocrSemantics();
                            var classDeclarations = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>();
                            var m = docrSemantics.GetSemantic(classDeclarations.ElementAt(0));

                            Debugger.Break();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Select an instance of Visual Studio.
        /// </summary>
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
