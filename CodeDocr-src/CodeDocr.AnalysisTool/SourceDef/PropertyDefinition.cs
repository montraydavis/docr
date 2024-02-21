namespace CodeDocr.AnalysisTool.SourceDef
{

    public class PropertyDefinition
    {
        public AttributeDefinition[] Attributes { get; }
        public string Name { get; }
        public string Type { get; }

        public PropertyDefinition(AttributeDefinition[] attributes, string name, string type)
        {
            Name = name;
            Type = type;

        }
    }

}
