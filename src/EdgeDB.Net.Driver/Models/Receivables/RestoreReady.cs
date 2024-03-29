﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#restoreready">Restore Ready</see> packet.
    /// </summary>
    public readonly struct RestoreReady : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.RestoreReady;

        /// <summary>
        ///     Gets a collection of annotations that was sent with this packet.
        /// </summary>
        public IReadOnlyCollection<Annotation> Annotations
            => _annotations.ToImmutableArray();

        /// <summary>
        ///     Gets the number of jobs that the restore will use.
        /// </summary>
        public ushort Jobs { get; }

        private readonly Annotation[] _annotations;

        internal RestoreReady(ref PacketReader reader)
        {
            _annotations = reader.ReadAnnotations();
            Jobs = reader.ReadUInt16();
        }
    }
}
