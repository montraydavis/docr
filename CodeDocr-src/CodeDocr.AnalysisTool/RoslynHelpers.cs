using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeDocr.AnalysisTool
{
    public static class RoslynHelpers
    {
        /// <summary>
        /// Gets the XML documentation comments directly preceding a method declaration.
        /// </summary>
        /// <param name="methodDeclaration">The method declaration to analyze.</param>
        /// <returns>A string representing the XML documentation comments found, or null if none exist.</returns>
        public static string GetMethodDocumentationComments(MethodDeclarationSyntax methodDeclaration)
        {
            // Get the leading trivia (spaces, comments, etc. before the method declaration).
            var leadingTrivia = methodDeclaration.GetLeadingTrivia();

            // Find the DocumentationCommentTriviaSyntax if it exists.
            var documentationTrivia = leadingTrivia
                .Select(trivia => trivia.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            var documentationText = documentationTrivia?.ToFullString();

            if (documentationText != null)
            {
                var documentationTexts = documentationTrivia.ToFullString()
                    .Split("\n")
                    .Where(l => l.Length > 0)
                    .Select(l =>
                    {

                        string pattern = @"(\s)+"; // Matches one or more whitespace characters

                        // Replace multiple spaces with a single space
                        string text = Regex.Replace(l, pattern, " ").Trim();

                        return text;
                    });

                var text = documentationTexts.Any()
                    ? documentationTexts.Aggregate((a, b) => a + "\n" + b)
                    : string.Empty;

                return text;

            }

            // If found, convert the XML documentation to string; otherwise, return null.
            return documentationText;
        }

        /// <summary>
        /// Gets the comments directly preceding a method declaration.
        /// </summary>
        /// <param name="methodDeclaration">The method declaration to analyze.</param>
        /// <returns>A list of strings representing the comments found.</returns>
        public static List<string> GetMethodPrecedingComments(MethodDeclarationSyntax methodDeclaration)
        {
            // Initialize a list to hold the comments.
            var comments = new List<string>();

            // Get the leading trivia (spaces, comments, etc. before the method declaration).
            var leadingTrivia = methodDeclaration.GetLeadingTrivia();

            // Filter the trivia to get only comments.
            var commentTrivia = leadingTrivia.Where(trivia =>
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));

            foreach (var trivia in commentTrivia)
            {
                // For single-line comments, just add the trivia text.
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    comments.Add(trivia.ToString());
                }
                // For multi-line comments, split by new lines to get individual lines of the comment.
                else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    var commentLines = trivia.ToString().Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
                    comments.AddRange(commentLines);
                }
            }

            return comments;
        }
    }
}
