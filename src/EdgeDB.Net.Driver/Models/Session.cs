using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.State
{
    public sealed class Session
    {
        public string Module { get; init; }
        public IReadOnlyDictionary<string, string> Aliases 
        {
            get => _aliases.ToImmutableDictionary();
            init => _aliases = value.ToDictionary(x => x.Key, x => x.Value);
        }
        public Config Config { get => _config; init => _config = value; }
        public IReadOnlyDictionary<string, object?> Globals
        {
            get => _globals.ToImmutableDictionary();
            init => _globals = value.ToDictionary(x => x.Key, x => x.Value);
        }
        
        private Dictionary<string, object?> _globals = new();
        private Dictionary<string, string> _aliases = new();
        private Config _config;

        public Session()
        {
            Module ??= "default";
            _config ??= new Config();
        }
   
        internal IDictionary<string, object?> Serialize()
        {
            var dict = new Dictionary<string, object?>();
            if(Module != "default")
                dict["module"] = Module;

            if(_aliases.Any())
                dict["aliases"] = Aliases;

            var serializedConfig = Config.Serialize();
            if(serializedConfig.Any())
                dict["config"] = serializedConfig;

            if(_globals.Any())
                dict["globals"] = Globals;
            
            return dict;
        }

        public Session WithGlobals(IDictionary<string, object?> globals)
        {
            _globals = globals.ToDictionary(x => x.Key, x => x.Value);
            return this;
        }

        public Session WithModuleAliases(IDictionary<string, string> aliases)
        {
            _aliases = aliases.ToDictionary(x => x.Key, x => x.Value);
            return this;
        }

        public Session WithConfig(Config config)
        {
            _config = config;
            return this;
        }

        public static Session Default
            => new();
    }
}
