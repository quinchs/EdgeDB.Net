﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class RestoreEOF : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.RestoreEOF;

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            // write nothing
        }
    }
}
