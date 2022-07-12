using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public sealed class ClientState
    {
        public string Module { get; } = "default";
        public IReadOnlyCollection<(string Alias, string Target)> Aliases { get; } = ImmutableArray<(string, string)>.Empty;
        public Config Config { get; } = new();
        public IReadOnlyCollection<(string Key, string Value)> Globals { get; } = ImmutableArray<(string, string)>.Empty;
    }
}
