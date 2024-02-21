using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeDocr.AnalysisTool.Helpers
{

    public class SyntaxHelper : ISyntaxHelper
    {
        // This method checks if classA references classB
        public bool HasClassReference(SyntaxTree tree, SemanticModel model, ClassDeclarationSyntax classA, ClassDeclarationSyntax classB)
        {
            // Get the symbol for classB from the semantic model
            var classBSymbol = model.GetDeclaredSymbol(classB);

            // Find all identifiers in classA
            var identifiers = classA.DescendantNodes().OfType<IdentifierNameSyntax>();

            foreach (var identifier in identifiers)
            {
                // Get the symbol for each identifier
                var symbol = model.GetSymbolInfo(identifier).Symbol;

                // If the symbol is a named type symbol, check if it matches classB's symbol
                if (symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol, classBSymbol))
                    {
                        return true; // Found a reference to classB in classA
                    }
                }
            }

            return false; // No reference to classB found in classA
        }
    }
}
