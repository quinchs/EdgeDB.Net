using System.Dynamic;

namespace EdgeDB.Codecs
{
    internal class Object : ICodec<object>, IArgumentCodec<object>
    {
        internal readonly ICodec[] InnerCodecs;
        internal readonly string[] PropertyNames;

        internal Object(ICodec[] innerCodecs, string[] propertyNames)
        {
            InnerCodecs = innerCodecs;
            PropertyNames = propertyNames;
        }

        public object? Deserialize(ref PacketReader reader)
        {
            var numElements = reader.ReadInt32();

            if (InnerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {InnerCodecs.Length} codecs");
            }

            dynamic data = new ExpandoObject();
            var dataDictionary = (IDictionary<string, object?>)data;

            for (int i = 0; i != numElements; i++)
            {
                // reserved
                reader.Skip(4);
                var name = PropertyNames[i];
                var length = reader.ReadInt32();

                if(length is -1)
                {
                    dataDictionary.Add(name, null);
                    continue;
                }

                reader.ReadBytes(length, out var innerData);

                object? value;

                value = InnerCodecs[i].Deserialize(innerData);

                dataDictionary.Add(name, value);
            }

            return data;
        }

        public void Serialize(PacketWriter writer, object? value)
        {
            throw new NotImplementedException();
        }

        public void SerializeArguments(PacketWriter writer, object? value)
        {
            object?[]? values = null;

            if (value is IDictionary<string, object?> dict)
                values = PropertyNames.Select(x => dict[x]).ToArray();
            else if (value is object?[] arr)
                value = arr;

            if (values is null)
            {
                throw new ArgumentException($"Expected dynamic object or array but got {value?.GetType()?.Name ?? "null"}");
            }

            using var innerWriter = new PacketWriter();
            for (int i = 0; i != values.Length; i++)
            {
                var element = values[i];
                var innerCodec = InnerCodecs[i];

                // reserved
                innerWriter.Write(0);

                // encode
                if (element is null)
                {
                    innerWriter.Write(-1);
                }
                else
                {
                    // special case for enums
                    if (element.GetType().IsEnum && innerCodec is Text)
                        element = element.ToString();

                    var elementBuff = innerCodec.Serialize(element);

                    innerWriter.Write(elementBuff.Length);
                    innerWriter.Write(elementBuff);
                }
            }

            writer.Write((int)innerWriter.BaseStream.Length + 4);
            writer.Write(values.Length);
            writer.Write(innerWriter);
        }
    }
}
