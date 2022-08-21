using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI;

internal interface ILanguageWriter
{
    public static ILanguageWriter GetWriter(string lang)
    {
        return lang switch
        {
            "c#" => new CSharpLanguageWriter()
        };
    }
}
