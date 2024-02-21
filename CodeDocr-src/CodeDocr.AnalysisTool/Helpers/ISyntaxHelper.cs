using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace CodeDocr.AnalysisTool.Helpers
{
    public interface ISyntaxHelper
    {
        bool HasClassReference(SyntaxTree tree, SemanticModel model, ClassDeclarationSyntax classA, ClassDeclarationSyntax classB);
    }
}
