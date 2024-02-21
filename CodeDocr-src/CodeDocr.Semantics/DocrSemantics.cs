using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodeDocr.Semantics
{
    /// <summary>
    /// Provides functionality for converting C# SyntaxNodes into human-readable descriptions.
    /// </summary>
    public class DocrSemantics
    {

        /// <summary>
        /// Removes the leading whitespace and triple forward slashes from the input string.
        /// </summary>
        /// <param name="input">The input string to be cleaned up.</param>
        /// <returns>The cleaned up string.</returns>
        public static string CleanupDocumentation(string input)
        {
            string pattern = @"\s*///";
            string replacement = "";
            string result = Regex.Replace(input, pattern, replacement).Trim(' ').Trim('\r').Trim('\t');
            return result;
        }

        /// <summary>
        /// Represents a service that converts C# SyntaxNodes into human-readable descriptions.
        /// </summary>
        public class SyntaxNodeDescriber
        {
            /// <summary>
            /// Generates a human-readable description for a given C# source code.
            /// </summary>
            /// <param name="sourceCode">The C# source code to describe.</param>
            /// <returns>A collection of string descriptions for each class and method in the source code.</returns>
            public IEnumerable<string> DescribeSource(string sourceCode)
            {
                var tree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = tree.GetRoot();

                var classNodes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classNode in classNodes)
                {
                    yield return DescribeClass(classNode);
                    foreach (var methodNode in classNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        yield return DescribeMethod(classNode.Identifier.Text, methodNode);
                    }
                }
            }


            /// <summary>
            /// Generates a description for the source code represented by the given syntax tree.
            /// </summary>
            /// <param name="rootNode">The root syntax node of the syntax tree.</param>
            /// <returns>An enumerable collection of descriptions for the source code.</returns>
            public IEnumerable<string> DescribeSource(SyntaxNode rootNode)
            {
                var classNodes = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classNode in classNodes)
                {
                    yield return DescribeClass(classNode);
                    foreach (var methodNode in classNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        yield return DescribeMethod(classNode.Identifier.Text, methodNode);
                    }
                }
            }

            /// <summary>
            /// Generates a description for a class.
            /// </summary>
            /// <param name="classNode">The class declaration syntax node.</param>
            /// <returns>A string description of the class.</returns>
            public string DescribeClass(ClassDeclarationSyntax classNode)
            {
                var hasParentNamespace = classNode.Ancestors().OfType<NamespaceDeclarationSyntax>().Any();
                var namespaceName = classNode.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();

                var classFullName = string.IsNullOrWhiteSpace(namespaceName)
                    ? classNode.Identifier.Text
                    : $"{namespaceName}.{classNode.Identifier.Text}";

                var className = classNode.Identifier.Text;

                var classDocumentation = classNode.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();

                var classProperyTexts = classNode.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                    .Where(p => p.Parent.IsKind(SyntaxKind.ClassDeclaration))
                    .Select(DescribeProperty)
                    .ToArray();

                var classFieldTexts = classNode.DescendantNodes().OfType<FieldDeclarationSyntax>()
                    .Where(p => p.Parent.IsKind(SyntaxKind.ClassDeclaration))
                    .Select(DescribeField)
                    .ToArray();

                var classMethods = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(p => p.Parent.IsKind(SyntaxKind.ClassDeclaration))
                    .Select(m => new KeyValuePair<MethodDeclarationSyntax, string>(m, DescribeMethod(className, m)))
                    .ToArray();

                var xmlText = "<root>" + classDocumentation?.ToString()
                    .Split('\n')
                    .Select(CleanupDocumentation)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Aggregate((current, next) => current + " " + next) + "</root>";

                var documentationText = ParseDocumentation(xmlText);

                var sb = new StringBuilder();

                var classHeaderName = hasParentNamespace
                    ? $"`{namespaceName}`.{className}"
                    : $"{className}";

                var classDescriptionText = "\n" + documentationText.Summary + "\n";

                sb.AppendLine($"# {classHeaderName}");
                sb.AppendLine(string.Empty);
                sb.AppendLine($"`description`: {documentationText.Summary}");
                sb.AppendLine(string.Empty);
                sb.AppendLine("## Properties");
                sb.AppendLine(string.Empty);

                foreach (var classProperty in classProperyTexts)
                {
                    sb.AppendLine(classProperty);
                    sb.AppendLine(string.Empty);
                }

                sb.AppendLine("## Fields");
                sb.AppendLine(string.Empty);

                foreach (var classField in classFieldTexts)
                {
                    sb.AppendLine(classField);
                    sb.AppendLine(string.Empty);
                }


                sb.AppendLine("## Methods");
                sb.AppendLine(string.Empty);

                foreach (var classMethod in classMethods)
                {

                    sb.AppendLine(classMethod.Value);

                    var methodBody = classMethod.Key.Body?.ToFullString()
                        .Split("\n")
                        .Select(l => l.Replace("\r",string.Empty).Trim(' '))
                        .Aggregate((a,b) => a+" "+b);

                    sb.AppendLine("<details>");
                    sb.AppendLine("<summary>Method Body</summary>");
                    sb.AppendLine($"<p>{methodBody}</p>");
                    sb.AppendLine("</details>");
                    sb.AppendLine(string.Empty);
                    sb.AppendLine(string.Empty);
                    sb.AppendLine("### Parameters");

                    var methodParameterTexts = classMethod.Key.ParameterList.Parameters
                        .Select(DescribeParameter)
                        .ToArray();

                    foreach (var methodParameterText in methodParameterTexts)
                    {
                        sb.AppendLine(methodParameterText);
                    }

                    if (methodParameterTexts.Any())
                        sb.AppendLine(string.Empty);
                }

                var text = sb.ToString();

                return text;
            }

            /// <summary>
            /// Describes a property by returning a formatted string containing the property name, modifiers, and type.
            /// </summary>
            /// <param name="propertyNode">The <see cref="PropertyDeclarationSyntax"/> representing the property.</param>
            /// <returns>A formatted string describing the property.</returns>
            private string DescribeProperty(PropertyDeclarationSyntax propertyNode)
            {
                var propertyName = propertyNode.Identifier.Text;
                var propertyType = propertyNode.Type.GetText().ToString().Trim(' ');
                var propertyDefaultSetValue = propertyNode.Initializer?.Value.GetText();
                var propertyModifiersText = propertyNode.Modifiers.Any()
                    ? $"`{propertyNode.Modifiers.Select(m => m.Text).Aggregate((current, next) => current + " " + next)}` "
                    : string.Empty;
                var propertyDocumentation = propertyNode.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();

                var summaryText = propertyDocumentation?.ToString()
                    .Split('\n')
                    .Select(CleanupDocumentation)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Aggregate((current, next) => current + " " + next);

                return $"**{propertyName}** is a {propertyModifiersText}property of type `{propertyType}`";
            }

            /// <summary>
            /// Describes a parameter by returning a formatted string containing the parameter name and type.
            /// </summary>
            /// <param name="parameterNode">The parameter syntax node.</param>
            /// <returns>A string describing the parameter.</returns>
            private string DescribeParameter(ParameterSyntax parameterNode)
            {
                var parameterName = parameterNode.Identifier.Text;
                var parameterType = parameterNode.Type?.GetText().ToString().Trim(' ') ?? "Unknown";

                return $"**{parameterName}** is a parameter of type `{parameterType}`";
            }

            /// <summary>
            /// Describes a field by extracting relevant information such as name, type, modifiers, default value, and documentation.
            /// </summary>
            /// <param name="fieldNode">The <see cref="FieldDeclarationSyntax"/> representing the field.</param>
            /// <returns>A string describing the field.</returns>
            private string DescribeField(FieldDeclarationSyntax fieldNode)
            {
                var fieldNodeName = fieldNode.Declaration.Variables.First().Identifier.Text;
                var fieldType = fieldNode.GetType().ToString();

                var fieldDefaultSetValue = fieldNode
                    .Declaration
                    .Variables
                    .First()
                    .Initializer?
                    .Value
                    .GetText()
                    .ToString();

                var fieldDocumentation = fieldNode.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();

                var fieldModifiersText = fieldNode.Modifiers.Any()
                    ? $"`{fieldNode.Modifiers.Select(m => m.Text).Aggregate((current, next) => current + " " + next)}` "
                    : string.Empty;

                var summaryText = fieldDocumentation?.ToString()
                    .Split('\n')
                    .Select(CleanupDocumentation)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Aggregate((current, next) => current + " " + next);

                var sb = new StringBuilder();
                sb.Append($"**{fieldNodeName}** is a {fieldModifiersText}field of type `{fieldType}`");

                if (!string.IsNullOrWhiteSpace(fieldDefaultSetValue))
                {
                    sb.Append($" with a default value of `{fieldDefaultSetValue}`.");
                }
                else
                {
                    sb.Append(".");
                }

                var text = sb.ToString();

                return text;
            }

            /// <summary>
            /// Generates a description for a method within a class.
            /// </summary>
            /// <param name="className">The name of the class containing the method.</param>
            /// <param name="methodNode">The method declaration syntax node.</param>
            /// <returns>A string description of the method.</returns>
            private string DescribeMethod(string className, MethodDeclarationSyntax methodNode)
            {
                var methodName = methodNode.Identifier.Text;
                var parameterCount = methodNode.ParameterList.Parameters.Count;
                var methodDocumentation = methodNode.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();

                var methodModifiersText = methodNode.Modifiers.Any()
                    ? $"`{methodNode.Modifiers.Select(m => m.Text).Aggregate((current, next) => current + " " + next)}` "
                    : string.Empty;

                var propertyDocumentation = methodNode.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();

                var xmlText = "<root>" + propertyDocumentation?.ToString()
                    .Split('\n')
                    .Select(CleanupDocumentation)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Aggregate((current, next) => current + " " + next) + "</root>";

                var documentationText = ParseDocumentation(xmlText);

                return $"**{className}.{methodName}** is a {methodModifiersText}method containing {parameterCount} parameter(s)\n\n{documentationText.Summary}";
            }


            /// <summary>
            ///Parse the given string as XML.
            /// </summary>
            private CSDocumentation ParseDocumentation(string xmlText)
            {
                var documentationXml = XDocument.Parse(xmlText);
                var summaryText = string.Empty;
                var returnTypeText = string.Empty;
                //var summaryText = string.Empty;


                if (documentationXml.Root is { } root)
                {
                    if (root.Element("summary") is { } summaryElement)
                    {
                        summaryText = summaryElement.Value.Trim(' ');
                    }

                    if (root.Element("returnType") is { } returnTypeElement)
                    {
                        summaryText = returnTypeElement.Value.Trim(' ');
                    }
                }

                var doc = new CSDocumentation(summaryText, null, returnTypeText);

                return doc;
            }

        }

        /// <summary>
        /// Gets the semantic information for the given syntax node.
        /// </summary>
        /// <param name="node">The syntax node to get semantic information for.</param>
        /// <returns>The semantic information of the syntax node.</returns>
        public string GetSemantic(SyntaxNode node)
        {
            var describer = new SyntaxNodeDescriber();

            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    var classNode = (ClassDeclarationSyntax)node;
                    var description = describer.DescribeClass(classNode);
                    return classNode.Identifier.ToString();
                case SyntaxKind.MethodDeclaration:
                    var methodNode = (MethodDeclarationSyntax)node;
                    return methodNode.Identifier.ToString();
                default:
                    return node.Kind().ToString();
            }
        }
    }
}
