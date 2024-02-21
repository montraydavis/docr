using ProtoBuf;
using System.IO;

namespace CodeDocr.AnalysisTool.Helpers.Data
{
    [ProtoContract]
    public class CSharpUsingDefinition
    {
        [ProtoMember(1)]
        public string Name { get; }
    }


    [ProtoContract]
    public class CSharpAttributeDefinition
    {
        [ProtoMember(1)]
        public string Name { get; }

        [ProtoMember(2)]
        public CSharpPropertyDefinition[] Arguments { get; }
    }

    [ProtoContract]
    public class CSharpPropertyDefinition
    {
        [ProtoMember(1)]
        public CSharpAttributeDefinition[] Attributes { get; }

        [ProtoMember(2)]
        public string Name { get; }

        [ProtoMember(3)]
        public string Type { get; }
    }

    [ProtoContract]
    public class CSharpDocumentationDefinition
    {
        [ProtoMember(1)]
        public string Comment { get; }
    }

    [ProtoContract]
    public class CSharpMethodDefinition
    {
        [ProtoMember(1)]
        public CSharpAttributeDefinition[] Attributes { get; }

        [ProtoMember(2)]
        public CSharpPropertyDefinition[] Parameters { get; }

        [ProtoMember(3)]
        public CSharpDocumentationDefinition Documentation { get; }

        [ProtoMember(4)]
        public string ReturnType { get; }
    }

    [ProtoContract]
    public class CSharpClassDefinition
    {
        [ProtoMember(1)]
        public CSharpUsingDefinition[] Usings { get; }

        [ProtoMember(2)]
        public CSharpPropertyDefinition[] Properties { get; }

        [ProtoMember(3)]
        public CSharpMethodDefinition[] Methods { get; }
    }
    public static class BindingDataSerializer
    {
        // Serializes a Person object to a file using protobuf-net.
        public static void Serialize<T>(T classDeclaration, string filePath)
        {
            using (var file = File.Create(filePath))
            {
                Serializer.Serialize(file, classDeclaration);
            }
        }

        // Deserializes a Person object from a file using protobuf-net.
        public static T Deserialize<T>(string filePath)
        {
            using (var file = File.OpenRead(filePath))
            {
                return Serializer.Deserialize<T>(file);
            }
        }
    }

    internal class DataHelper
    {

    }
}
