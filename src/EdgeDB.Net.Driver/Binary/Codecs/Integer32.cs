namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer32 : IScalarCodec<int>
    {
        public int Deserialize(ref PacketReader reader)
        {
            return reader.ReadInt32();
        }

        public void Serialize(ref PacketWriter writer, int value)
        {
            writer.Write(value);
        }
    }
}
