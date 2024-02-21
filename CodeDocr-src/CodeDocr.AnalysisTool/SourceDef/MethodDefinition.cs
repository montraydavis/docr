namespace CodeDocr.AnalysisTool.SourceDef
{
    public class MethodDefinition
    {
        public string Name { get; }
        public AttributeDefinition[] Attributes { get; }
        public PropertyDefinition[] Parameters { get; }
        public DocumentationDefinition Documentation { get; }
        public string ReturnType { get; }

        public bool IsPublic { get; }
        public bool IsStatic { get; }
        public bool IsVirtual { get; }
        public bool IsOverride { get; }
        public bool IsAbstract { get; }
        public bool IsExtern { get; }
        public bool IsInternal { get; }
        public bool IsProtected { get; }
        public bool IsPrivate { get; }

        public MethodDefinition(string name, string returnType,
            AttributeDefinition[] attributes, PropertyDefinition[] parameters, DocumentationDefinition documentation, 
            bool isPublic, bool isStatic, 
            bool isVirtual, bool isOverride, bool isAbstract,
            bool isExtern, bool isInternal, bool isProtected,
            bool isPrivate)
        {
            Name = name;
            IsPublic = isPublic;
            IsStatic = isStatic;
            IsVirtual = isVirtual;
            IsOverride = isOverride;
            IsAbstract = isAbstract;
            IsExtern = isExtern;
            IsInternal = isInternal;
            IsProtected = isProtected;
            IsPrivate = isPrivate;
            Attributes = attributes;
            Parameters = parameters;
            Documentation = documentation;
            ReturnType = returnType;
        }
    }

}
