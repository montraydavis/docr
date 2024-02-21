using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocr.AnalysisTool.SourceDef
{

    public class NamespaceDefinition
    {
        public string Name { get; }
        public ClassDefinition[] Classes { get; }

        public NamespaceDefinition(NamespaceDeclarationSyntax name, ClassDefinition[] classes)
        {
            if (name != null)
                Name = name.Name.ToString();

            Classes = classes;
        }
    }

}
