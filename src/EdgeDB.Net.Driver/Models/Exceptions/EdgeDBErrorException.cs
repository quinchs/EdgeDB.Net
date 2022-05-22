﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that wraps <see cref="Models.ErrorResponse"/>.
    /// </summary>
    public sealed class EdgeDBErrorException : EdgeDBException
    {
        public string? Details { get; }
        public string? ServerTraceBack { get; }
        public string? Hint { get; }
        public Models.ErrorResponse ErrorResponse { get; }

        public EdgeDBErrorException(Models.ErrorResponse error)
            : base(error.Message, typeof(ServerErrorCodes).GetField(error.ErrorCode.ToString())?.IsDefined(typeof(ShouldRetryAttribute), false) ?? false)
        {
            if(error.Headers.Any(x => x.Code == 0x0002))
                Details = Encoding.UTF8.GetString(error.Headers.FirstOrDefault(x => x.Code == 0x0002).Value);

            if (error.Headers.Any(x => x.Code == 0x0101))
                ServerTraceBack = Encoding.UTF8.GetString(error.Headers.FirstOrDefault(x => x.Code == 0x0101).Value);

            if (error.Headers.Any(x => x.Code == 0x0001))
                Hint = Encoding.UTF8.GetString(error.Headers.FirstOrDefault(x => x.Code == 0x0001).Value);

            ErrorResponse = error;
        }
    }
}
