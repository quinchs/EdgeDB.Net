using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    internal class HashUtils
    {
        public static string HashEdgeQL(string edgeql)
        {
            using var algo = SHA256.Create();
            return HexConverter.ToHex(algo.ComputeHash(Encoding.UTF8.GetBytes(edgeql)));
        }
    }
}
