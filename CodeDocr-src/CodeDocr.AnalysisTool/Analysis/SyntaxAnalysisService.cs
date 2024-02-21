using CodeDocr.AnalysisTool.Helpers;
using CodeDocr.AnalysisTool.SourceDef;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace CodeDocr.AnalysisTool.Analysis
{
    public class SyntaxAnalysisService : ISyntaxAnalysisService
    {
        private readonly ISyntaxHelper syntaxHelper;

        public UsingDefinition[] ParseUsingsDefinitions(SyntaxNode node)
        {
            var usingDefinitions = node.DescendantNodes().OfType<UsingDirectiveSyntax>()
                .Select(u => new UsingDefinition(u.Name.GetText().ToString()))
                .ToArray();

            return usingDefinitions;
        }

        public ClassPropertyDefinition[] ParseClassPropertyDefinitions(ClassDeclarationSyntax classDeclaration)
        {
            var propertyDefinitions = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().Select(p =>
            {
                var propertyAttributes = p.AttributeLists.SelectMany(a => a.Attributes.Select(classAttribute =>
                {
                    var propertyAttributeArgumentList = classAttribute.ArgumentList.Arguments.Select(ar =>
                    {
                        return new PropertyDefinition(new AttributeDefinition[0], ar.NameEquals.Name.ToString(), ar.Expression.ToString());
                    }).ToArray();

                    return new AttributeDefinition(classAttribute.Name.ToString(), propertyAttributeArgumentList);
                })).ToArray();


                var propertyName = p.Identifier.Text;
                var propertyType = p.Type.ToString();
                var propertyIsPublic = p.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword);
                var propertyIsStatic = p.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword);
                var propertyIsReadOnly = p.Modifiers.Any(m => m.Kind() == SyntaxKind.ReadOnlyKeyword);
                var propertyIsVirtual = p.Modifiers.Any(m => m.Kind() == SyntaxKind.VirtualKeyword);
                var propertyIsOverride = p.Modifiers.Any(m => m.Kind() == SyntaxKind.OverrideKeyword);
                var propertyIsAbstract = p.Modifiers.Any(m => m.Kind() == SyntaxKind.AbstractKeyword);
                var propertyIsExtern = p.Modifiers.Any(m => m.Kind() == SyntaxKind.ExternKeyword);
                var propertyIsConst = p.Modifiers.Any(m => m.Kind() == SyntaxKind.ConstKeyword);
                var propertyIsInternal = p.Modifiers.Any(m => m.Kind() == SyntaxKind.InternalKeyword);
                var propertyIsProtected = p.Modifiers.Any(m => m.Kind() == SyntaxKind.ProtectedKeyword);
                var propertyIsPrivate = p.Modifiers.Any(m => m.Kind() == SyntaxKind.PrivateKeyword) && p.Modifiers.All(m => m.Kind() != SyntaxKind.PublicKeyword);

                return new ClassPropertyDefinition(propertyAttributes, propertyName, propertyType,
                    propertyIsPublic, propertyIsStatic, propertyIsReadOnly,
                    propertyIsVirtual, propertyIsOverride,
                    propertyIsAbstract, propertyIsExtern, propertyIsConst,
                    propertyIsInternal, propertyIsProtected, propertyIsPrivate);
            }).ToArray();

            return propertyDefinitions;
        }

        public PropertyDefinition[] ParsePropertyDefinitions(ClassDeclarationSyntax classDeclaration)
        {
            var propertyDefinitions = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().Select(p =>
            {
                var propertyAttributes = p.AttributeLists.SelectMany(a => a.Attributes.Select(classAttribute =>
                {
                    var propertyAttributeArgumentList = classAttribute.ArgumentList.Arguments.Select(ar =>
                    {
                        return new PropertyDefinition(new AttributeDefinition[0], ar.NameEquals.Name.ToString(), ar.Expression.ToString());
                    }).ToArray();

                    return new AttributeDefinition(classAttribute.Name.ToString(), propertyAttributeArgumentList);
                })).ToArray();

                var propertyName = p.Identifier.Text;
                var propertyType = p.Type.ToString();

                return new PropertyDefinition(propertyAttributes, propertyName, propertyType);
            }).ToArray();

            return propertyDefinitions;
        }

        public PropertyDefinition[] ParseMethodParameters(MethodDeclarationSyntax methodDeclaration)
        {
            var parameters = methodDeclaration.ParameterList.Parameters.Select(p =>
            {
                var parameterAttributes = p.AttributeLists.SelectMany(a => a.Attributes.Select(at =>
                {
                    var parameterAttributeArgumentList = at.ArgumentList.Arguments.Select(ar =>
                    {
                        return new PropertyDefinition(new AttributeDefinition[0], ar.NameEquals.Name.ToString(), ar.Expression.ToString());
                    }).ToArray();

                    return new AttributeDefinition(at.Name.ToString(), parameterAttributeArgumentList);
                })).ToArray();

                var parameterName = p.Identifier.Text;
                var parameterType = p.Type.ToString();

                return new PropertyDefinition(parameterAttributes, parameterName, parameterType);
            }).ToArray();

            return parameters;
        }

        public AttributeDefinition[] ParseAttributeDefinitions(SyntaxNode declaration)
        {
            var attributeDefinitions = declaration.DescendantNodes().OfType<AttributeListSyntax>().SelectMany(a => a.Attributes.Select(at =>
            {
                return new AttributeDefinition(at.Name.ToString(), at.ArgumentList.Arguments.Select(ar =>
                {
                    return new PropertyDefinition(new AttributeDefinition[0], ar.NameEquals.Name.ToString(), ar.Expression.ToString());
                }).ToArray());
            })).ToArray();

            return attributeDefinitions;
        }

        public MethodDefinition[] ParseMethodDefinitions(SyntaxNode declaration)
        {
            var methodDefinitions = declaration.DescendantNodes().OfType<MethodDeclarationSyntax>().Select(m =>
            {
                var parameters = ParseMethodParameters(m);

                var methodAttributes = m.AttributeLists.SelectMany(a => a.Attributes.Select(at =>
                {
                    var attributeName = at.Name.ToString();
                    var attributeArgumentList = at.ArgumentList.Arguments
                        .Where(ar => ar.NameEquals != null).ToArray();

                    return new AttributeDefinition(attributeName, attributeArgumentList.Select(ar =>
                    {
                        return new PropertyDefinition(new AttributeDefinition[0], ar.NameEquals.Name.ToString(), ar.Expression.ToString());
                    }).ToArray());
                })).ToArray();

                var docComments = RoslynHelpers.GetMethodDocumentationComments(m);
                var documentation = new DocumentationDefinition(docComments);
                var returnType = m.ReturnType.ToString();
                var propertyName = m.Identifier.Text;

                var methodIsPublic = m.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword);
                var methodIsStatic = m.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword);
                var methodIsReadOnly = m.Modifiers.Any(m => m.Kind() == SyntaxKind.ReadOnlyKeyword);
                var methodIsVirtual = m.Modifiers.Any(m => m.Kind() == SyntaxKind.VirtualKeyword);
                var methodIsOverride = m.Modifiers.Any(m => m.Kind() == SyntaxKind.OverrideKeyword);
                var methodIsAbstract = m.Modifiers.Any(m => m.Kind() == SyntaxKind.AbstractKeyword);
                var methodIsExtern = m.Modifiers.Any(m => m.Kind() == SyntaxKind.ExternKeyword);
                var methodIsConst = m.Modifiers.Any(m => m.Kind() == SyntaxKind.ConstKeyword);
                var methodIsInternal = m.Modifiers.Any(m => m.Kind() == SyntaxKind.InternalKeyword);
                var methodIsProtected = m.Modifiers.Any(m => m.Kind() == SyntaxKind.ProtectedKeyword);
                var methodIsPrivate = m.Modifiers.Any(m => m.Kind() == SyntaxKind.PrivateKeyword) && m.Modifiers.All(m => m.Kind() != SyntaxKind.PublicKeyword);

                return new MethodDefinition(m.Identifier.Text, returnType, methodAttributes, parameters,
                    documentation, methodIsPublic, methodIsStatic, methodIsVirtual, methodIsOverride,
                    methodIsAbstract, methodIsExtern, methodIsInternal, methodIsProtected, methodIsPrivate);

            }).ToArray();

            return methodDefinitions;
        }

        public ClassDefinition[] ParseClassDefinitions(SyntaxTree tree, SemanticModel semanticModel)
        {
            var rootNode = tree.GetRoot();
            var classDeclarations = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .ToArray();
            var classes = classDeclarations.Select(classDeclaration =>
            {
                var classUsings = ParseUsingsDefinitions(rootNode);

                var classProperties = ParseClassPropertyDefinitions(classDeclaration);

                var methodDefinitions = ParseMethodDefinitions(classDeclaration);

                var refList = new List<string>();

                foreach (var c in classDeclarations)
                {
                    if (c != classDeclaration)
                    {
                        var classHasReference = this.syntaxHelper.HasClassReference(tree, semanticModel, c, classDeclaration);

                        if (classHasReference)
                        {
                            refList.Add(c.Identifier.Text);
                        }
                    }
                }

                return new ClassDefinition(classDeclaration.Identifier.Text, classUsings, classProperties, methodDefinitions);
            }).ToArray();

            return classes;
        }

        public SyntaxAnalysisService(ISyntaxHelper helper)
        {
            this.syntaxHelper = helper;
        }

    }
}
