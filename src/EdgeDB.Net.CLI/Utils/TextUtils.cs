using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    internal class TextUtils
    {
        private static CultureInfo? _cultureInfo;

        public static string ToPascalCase(string input)
        {
            _cultureInfo ??= CultureInfo.CurrentCulture;

            return _cultureInfo.TextInfo.ToTitleCase(input.Replace("_", " ")).Replace(" ", "");
        }

        public static string ToCamelCase(string input)
        {
            var p = ToPascalCase(input);
            return $"{p[0].ToString().ToLower()}{p[1..]}";
        }
    }
}
