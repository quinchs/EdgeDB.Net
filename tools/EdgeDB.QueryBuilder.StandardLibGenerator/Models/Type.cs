using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryBuilder.StandardLibGenerator.Models
{
    [EdgeDBType(ModuleName = "schema")]
    internal class Type
    {
        public string? Name { get; set; }
        public bool IsAbstract { get; set; }
        
        [EdgeDBProperty("expr")]
        public string? Expression { get; set; }
    }
}
