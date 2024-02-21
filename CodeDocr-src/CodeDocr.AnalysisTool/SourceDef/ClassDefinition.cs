namespace CodeDocr.AnalysisTool.SourceDef
{
    public class ClassDefinition
    {
        public string Name { get; }
        public UsingDefinition[] Usings { get; }
        public ClassPropertyDefinition[] Properties { get; }
        public MethodDefinition[] Methods { get; }

        public ClassDefinition(string name, UsingDefinition[] usings, ClassPropertyDefinition[] properties, MethodDefinition[] methods)
        {
            Name = name;
            Usings = usings;
            Properties = properties;
            Methods = methods;
        }
    }

}
