namespace CodeDocr.Semantics
{
    /// <summary>
    /// Represents a documentation object for C# code.
    /// </summary>
    public class CSDocumentation
    {
        /// <summary>
        /// Gets the summary of the code.
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Gets the parameters of the code.
        /// </summary>
        public string[] Param { get; }

        /// <summary>
        /// Gets the return type of the code.
        /// </summary>
        public string ReturnType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSDocumentation"/> class.
        /// </summary>
        /// <param name="summary">The summary of the code.</param>
        /// <param name="param">The parameters of the code.</param>
        /// <param name="returnType">The return type of the code.</param>
        public CSDocumentation(string summary, string[]? param = null, string? returnType = null)
        {
            Summary = summary;
            Param = param ?? new string[0];
            ReturnType = returnType ?? "void";
        }
    }
}
