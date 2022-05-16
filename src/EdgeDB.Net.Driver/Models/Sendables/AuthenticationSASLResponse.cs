﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal class AuthenticationSASLResponse : Sendable
    {
        public override ClientMessageTypes Type 
            => ClientMessageTypes.AuthenticationSASLResponse;

        private readonly byte[] _payload;

        public AuthenticationSASLResponse(byte[] payload)
        {
            _payload = payload;
        }

        protected override void BuildPacket(PacketWriter writer, EdgeDBBinaryClient client)
        {
            writer.WriteArray(_payload);
        }
    }
}
