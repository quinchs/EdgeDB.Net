using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public sealed class Session
    {
        public string Module { get; init; }
        public IReadOnlyDictionary<string, string> Aliases { get; init; }
        public Config Config { get; init; }
        public IReadOnlyDictionary<string, object?> Globals { get; init; }

        public Session()
        {
            Module = "default";
            Aliases ??= ImmutableDictionary<string, string>.Empty;
            Config ??= new Config();
            Globals ??= ImmutableDictionary<string, object?>.Empty;
        }
   
        internal IDictionary<string, object?> Serialize()
        {
            var dict = new Dictionary<string, object?>();
            if(Module != "default")
                dict["module"] = Module;

            if(Aliases.Any())
                dict["aliases"] = Aliases;

            var serializedConfig = Config.Serialize();
            if(serializedConfig.Any())
                dict["config"] = serializedConfig;

            if(Globals.Any())
                dict["globals"] = Globals;
            
            return dict;
        }
    }
}
