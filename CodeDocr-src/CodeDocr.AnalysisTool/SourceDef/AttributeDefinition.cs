namespace CodeDocr.AnalysisTool.SourceDef
{
    public class AttributeDefinition
    {
        public string Name { get; }
        public PropertyDefinition[] Arguments { get; }

        public AttributeDefinition(string name, PropertyDefinition[] arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }

}
