﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class Terminate : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.Terminate;

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client) { } // no data
    }
}
