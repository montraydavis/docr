using CodeDocr.AnalysisTool.SourceDef;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocr.AnalysisTool.Analysis
{
    public interface ISyntaxAnalysisService
    {
        UsingDefinition[] ParseUsingsDefinitions(SyntaxNode node);
        PropertyDefinition[] ParsePropertyDefinitions(ClassDeclarationSyntax classDeclaration);
        PropertyDefinition[] ParseMethodParameters(MethodDeclarationSyntax methodDeclaration);
        AttributeDefinition[] ParseAttributeDefinitions(SyntaxNode declaration);
        MethodDefinition[] ParseMethodDefinitions(SyntaxNode declaration);
        ClassDefinition[] ParseClassDefinitions(SyntaxTree tree, SemanticModel semanticModel);
    }
}
