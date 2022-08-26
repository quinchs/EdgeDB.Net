using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    internal class TextUtils
    {
        private static CultureInfo? _cultureInfo;

        public static string ToPascalCase(string input)
        {
            _cultureInfo ??= CultureInfo.CurrentCulture;
            var t = Regex.Replace(input, @"[^^]([A-Z])", m => $"{m.Value[0]} {m.Groups[1].Value}");
            
            return _cultureInfo.TextInfo.ToTitleCase(t.Replace("_", " ")).Replace(" ", "");
        }

        public static string ToCamelCase(string input)
        {
            var p = ToPascalCase(input);
            return $"{p[0].ToString().ToLower()}{p[1..]}";
        }
    }
}
