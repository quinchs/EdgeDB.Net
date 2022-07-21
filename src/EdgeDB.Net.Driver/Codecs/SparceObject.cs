using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    internal class SparceObject : ICodec<object>, IArgumentCodec<object>
    {
        private readonly ICodec[] _innerCodecs;
        private readonly ShapeElement[] _shape;

        internal SparceObject(ICodec[] innerCodecs, ShapeElement[] shape)
        {
            _innerCodecs = innerCodecs;
            _shape = shape;
        }

        public object? Deserialize(ref PacketReader reader)
        {
            var numElements = reader.ReadInt32();

            if (_innerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {_innerCodecs.Length} codecs");
            }

            dynamic data = new ExpandoObject();
            var dataDictionary = (IDictionary<string, object?>)data;

            for (int i = 0; i != numElements; i++)
            {
                var index = reader.ReadInt32();
                var shapeElement = _shape[index];

                var length = reader.ReadInt32();

                if (length is -1)
                {
                    dataDictionary.Add(shapeElement.Name, null);
                    continue;
                }

                reader.ReadBytes(length, out var innerData);

                object? value;

                value = _innerCodecs[i].Deserialize(innerData);

                dataDictionary.Add(shapeElement.Name, value);
            }

            return data;
        }

        public void Serialize(PacketWriter writer, object? value)
        {
            writer.Write(0);
        }

        public void SerializeArguments(PacketWriter writer, object? value)
        {
            
        }
    }
}
