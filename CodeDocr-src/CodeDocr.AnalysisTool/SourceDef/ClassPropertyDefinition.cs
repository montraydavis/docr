namespace CodeDocr.AnalysisTool.SourceDef
{
    public class ClassPropertyDefinition : PropertyDefinition
    {
        public bool IsPublic { get; }
        public bool IsStatic { get; }
        public bool IsReadOnly { get; }
        public bool IsVirtual { get; }
        public bool IsOverride { get; }
        public bool IsSealed { get; }
        public bool IsAbstract { get; }
        public bool IsExtern { get; }
        public bool IsUnsafe { get; }
        public bool IsPartial { get; }
        public bool IsConst { get; }
        public bool IsVolatile { get; }
        public bool IsNew { get; }
        public bool IsInternal { get; }
        public bool IsProtected { get; }
        public bool IsPrivate { get; }

        public ClassPropertyDefinition(AttributeDefinition[] attributes,
            string name,
            string type,
            bool isPublic, bool isStatic, bool isReadOnly,
            bool isVirtual, bool isOverride,
            bool isAbstract, bool isExtern, bool isConst,
            bool isInternal, bool isProtected, bool isPrivate) : base(attributes, name, type)
        {
            IsPublic = isPublic;
            IsStatic = isStatic;
            IsReadOnly = isReadOnly;
            IsVirtual = isVirtual;
            IsOverride = isOverride;
            IsAbstract = isAbstract;
            IsExtern = isExtern;
            IsConst = isConst;
            IsInternal = isInternal;
            IsProtected = isProtected;
            IsPrivate = isPrivate;
        }
    }

}
