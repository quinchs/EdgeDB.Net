﻿using System.Reflection;

namespace EdgeDB.Codecs
{
    internal interface IArgumentCodec<TType> : IArgumentCodec, ICodec<TType>
    {
        void SerializeArguments(PacketWriter writer, TType? value);
    }

    internal interface IArgumentCodec
    {
        void SerializeArguments(PacketWriter writer, object? value);

        byte[] SerializeArguments(object? value)
        {
            using var writer = new PacketWriter();
            SerializeArguments(writer, value);
            return writer.GetBytes();
        }
    }

    internal interface ICodec<TConverter> : ICodec
    {
        void Serialize(PacketWriter writer, TConverter? value);

        new TConverter? Deserialize(ref PacketReader reader);

        new TConverter? Deserialize(byte[] buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }
        
        new TConverter? Deserialize(Span<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }

        byte[] Serialize(TConverter? value)
        {
            var writer = new PacketWriter();
            Serialize(writer, value);
            return writer.GetBytes();
        }

        // ICodec
        object? ICodec.Deserialize(ref PacketReader reader) 
            => Deserialize(ref reader);

        void ICodec.Serialize(PacketWriter writer, object? value) 
            => Serialize(writer, (TConverter?)value);

        Type ICodec.ConverterType 
            => typeof(TConverter);

        bool ICodec.CanConvert(Type t)
            => t == typeof(TConverter);
    }

    internal interface ICodec
    {
        bool CanConvert(Type t);

        Type ConverterType { get; }

        void Serialize(PacketWriter writer, object? value);

        object? Deserialize(ref PacketReader reader);

        object? Deserialize(Span<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }

        object? Deserialize(byte[] buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }

        byte[] Serialize(object? value)
        {
            var writer = new PacketWriter();
            Serialize(writer, value);
            return writer.GetBytes();
        }

        private static readonly List<ICodec> _codecs;

        static ICodec()
        {
            _codecs = new();

            var codecs = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Any(x => x.Name == "IScalarCodec`1"));

            foreach (var codec in codecs)
            {
                // create instance
                var inst = (ICodec)Activator.CreateInstance(codec)!;

                _codecs.Add(inst);
            }
        }

        static IScalarCodec<TType>? GetScalarCodec<TType>()
            => (IScalarCodec<TType>?)_codecs.FirstOrDefault(x => x.ConverterType == typeof(TType) || x.CanConvert(typeof(TType)));
    }

    internal interface IScalarCodec<TInner> : ICodec<TInner> { }
}
