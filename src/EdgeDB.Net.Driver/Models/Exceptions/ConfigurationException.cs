using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class ConfigurationException : EdgeDBException
    {
        public ConfigurationException(string message) : base(message) { }
    }
}
