﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents the configuration options for a <see cref="EdgeDBClient"/> or <see cref="EdgeDBTcpClient"/>
    /// </summary>
    public class EdgeDBConfig
    {
        /// <summary>
        ///     Gets or sets whether or not to allow an unsecure connection when connecting over TCP.
        /// </summary>
        public bool AllowUnsecureConnection { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether or not the servers signature has to match the <see cref="EdgeDBConnection.TLSCertData"/> 
        ///     and <see cref="EdgeDBConnection.TLSCA"/>.
        /// </summary>
        /// <remarks>
        ///     This option is by default <see langword="true"/>.
        /// </remarks>
        public bool RequireCertificateMatch { get; set; } = true;

        /// <summary>
        ///     Gets or sets the default client pool size.
        /// </summary>
        public int DefaultPoolSize { get; set; } = 50;

        /// <summary>
        ///     Gets or sets the logger used for logging messages from the driver.
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        ///     Gets or sets the retry mode for connecting new clients.
        /// </summary>
        public ConnectionRetryMode RetryMode { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of times to retry to connect.
        /// </summary>
        public uint MaxConnectionRetries { get; set; } = 5;

        /// <summary>
        ///     Gets or sets the max amount of miliseconds a client will wait for an expected message.
        /// </summary>
        public uint MessageTimeout { get; set; } = 5000;
    }
}