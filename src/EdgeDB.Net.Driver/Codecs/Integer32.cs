﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    internal class Integer32 : IScalarCodec<int>
    {
        public int Deserialize(PacketReader reader)
        {
            return reader.ReadInt32();
        }

        public void Serialize(PacketWriter writer, int value)
        {
            writer.Write(value);
        }
    }
}