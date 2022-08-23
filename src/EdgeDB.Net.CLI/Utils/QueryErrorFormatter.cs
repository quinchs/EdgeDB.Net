using EdgeDB;
using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    internal class QueryErrorFormatter
    {
        public static string FormatError(string query, EdgeDBErrorException error)
        {
            var headers = error.ErrorResponse.Attributes.Cast<KeyValue?>();
            var rawCharStart = headers.FirstOrDefault(x => x.HasValue && x.Value.Code == 0xFFF9, null);
            var rawCharEnd = headers.FirstOrDefault(x => x.HasValue && x.Value.Code == 0xFFFA, null);

            if (!rawCharStart.HasValue || !rawCharEnd.HasValue)
                return EdgeDBColorer.ColorSchemaOrQuery(query);

            int charStart = int.Parse(rawCharStart.Value.ToString()), 
                charEnd = int.Parse(rawCharEnd.Value.ToString());

            var queryErrorSource = query[charStart..charEnd];
            var count = charEnd - charStart;
            
            var coloredQuery = EdgeDBColorer.ColorSchemaOrQuery(query, charStart..charEnd);

            // make the error section red
            var coloredIndex = coloredQuery.IndexOf(queryErrorSource, charStart);
            coloredQuery = coloredQuery.Remove(coloredIndex, count);
            coloredQuery = coloredQuery.Insert(coloredIndex, $"\u001b[0;31m{queryErrorSource}");
            return coloredQuery + "\u001b[0m";
        }
    }
}
