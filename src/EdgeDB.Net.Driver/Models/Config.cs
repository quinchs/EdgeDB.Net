using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public enum DDLPolicy
    {
        AlwaysAllow,
        NeverAllow,
    }

    public sealed class Config
    {
        public TimeSpan IdleTransationTimeout { get; }
        public TimeSpan QueryExecutionTimeout { get; }
        public bool AllowDMLInFunctions { get; }
        public DDLPolicy AllowBareDDL { get; }
        public bool ApplyAccessPolicies { get; }

        internal Config()
        {
            IdleTransationTimeout = TimeSpan.FromSeconds(10);
            QueryExecutionTimeout = TimeSpan.Zero;
            AllowDMLInFunctions = false;
            AllowBareDDL = DDLPolicy.AlwaysAllow;
            ApplyAccessPolicies = true;
        }
    }

    public sealed class ConfigProperties
    {
        public Optional<TimeSpan> IdleTransationTimeout { get; set; }
        public Optional<TimeSpan> QueryExecutionTimeout { get; set; }
        public Optional<bool> AllowDMLInFunctions { get; set; }
        public Optional<DDLPolicy> AllowBareDDL { get; set; }
        public Optional<bool> ApplyAccessPolicies { get; set; }
    }
}
