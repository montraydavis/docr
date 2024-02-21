using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CodeDocr
{
    public class ClassAnalysis
    {
        /// <summary>
        /// Parses the given C# code and returns the syntax tree.
        /// </summary>
        /// <param name="code">The C# code to parse.</param>
        /// <returns>The syntax tree representing the parsed code.</returns>
        public static SyntaxTree ParseCSharpText(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            return syntaxTree;
        }

        /// <summary>
        /// Reads the using directives from the given syntax tree.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to read the using directives from.</param>
        /// <returns>A list of using directives.</returns>
        public static List<string> ReadUsingsFromSyntaxTree(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            var usings = usingDirectives.Select(u => u.Name.ToString()).ToList();

            //var resp = System.Diagnostics.Process.Start("cmd", "gh copilot explain \"sudo apt-get\"");

            // Execute comand prompt command
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c gh explain \"sudo apt-get\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            //startInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();


            return usings;
        }
    }


}