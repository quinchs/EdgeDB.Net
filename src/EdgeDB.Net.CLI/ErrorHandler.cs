using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI
{
    internal class ErrorHandler
    {
        public static void HandleErrors(ParserResult<object> result, IEnumerable<Error> errors)
        {
            foreach(var error in errors)
            {
                switch (error)
                {
                    case NoVerbSelectedError noVerbs:
                        {
                            Console.WriteLine("No command specified use --help to view a list of commands");
                        }
                        break;
                    default:
                        HelpText.AutoBuild(result, h =>
                        {
                            h.Heading = "EdgeDB.Net CLI";
                            h.Copyright = "EdgeDB (c) 2022 edgedb.com";
                            return h;
                        });
                        break;
                }
            }
        }
    }
}
