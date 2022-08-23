using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    internal class EdgeDBColorer
    {
        public const int ANSI_COLOR_RED = 31;
        public const int ANSI_COLOR_GREEN = 32;
        public const int ANSI_COLOR_YELLOW = 33;
        public const int ANSI_COLOR_BLUE = 34;
        public const int ANSI_COLOR_PINK = 35;
        public const int ANSI_COLOR_WHITE = 37;

        public static string ColorSchemaOrQuery(string edgeql, Range ignore)
        {
            var lIndent = 0;
            var left = edgeql[..ignore.Start];
            var right = edgeql[ignore.End..];
            var unmarkedContent = edgeql[ignore];
            return ColorSchemaOrQuery(left, ref lIndent) + unmarkedContent + ColorSchemaOrQuery(right, ref lIndent);
        }

        public static string ColorSchemaOrQuery(string edgeql)
        {
            int i = 0;
            return ColorSchemaOrQuery(edgeql, ref i);
        }

        public static string ColorSchemaOrQuery(string edgeql, ref int indentLevel)
        {
            var mutableSchema = edgeql;
            var coloredSchema = "";
            var currentColor = 0;

            void ChangeColor(int color)
            {
                if (currentColor == color)
                    return;

                coloredSchema += $"\u001b[0;{color}m";
                currentColor = color;
            }

            while (!string.IsNullOrEmpty(mutableSchema))
            {
                var word = new string(mutableSchema.TakeWhile(x => !(x is '<' or '>' or '\"' or '\'' or ',' or '{' or '}' or '[' or ']' or ' ' or '(' or ')' or ';' or ':')).ToArray());

                if (string.IsNullOrEmpty(word))
                    word = $"{mutableSchema.FirstOrDefault('\u0000')}";

                if (word == "\u0000")
                    break;

                mutableSchema = mutableSchema[(word.Length)..];

                var lower = word.ToLower();

                if(Regex.IsMatch(word, @"^\d+$"))
                    ChangeColor(ANSI_COLOR_GREEN);
                else if (word == "<")
                {
                    int d = 0;
                    var str = new string(mutableSchema.TakeWhile((c, i) =>
                    {
                        if (c == '<')
                            d++;
                        else if (c == '>' && d == 0)
                            return false;
                        else if (c == '>')
                            d--;
                        return true;
                            
                    }).ToArray());
                    word += str + mutableSchema[str.Length];
                    ChangeColor(ANSI_COLOR_RED);
                    mutableSchema = mutableSchema[(word.Length - 1)..];
                }
                else if (word == "\"")
                {
                    var str = new string(mutableSchema.TakeWhile((c, i) => !(c is '\"' && mutableSchema[i - 1] is not '\\')).ToArray());
                    word += str + mutableSchema[str.Length];
                    ChangeColor(ANSI_COLOR_YELLOW);
                    mutableSchema = mutableSchema[(word.Length - 1)..];
                }
                else if (word == "\'")
                {
                    var str = new string(mutableSchema.TakeWhile((c, i) => !(c is '\'' && mutableSchema[i - 1] is not '\\')).ToArray());
                    word += str + mutableSchema[str.Length];
                    ChangeColor(ANSI_COLOR_YELLOW);
                    mutableSchema = mutableSchema[(word.Length - 1)..];
                }
                else if (word == " " || word == "\n") { }
                else if (word is "{" or "}" or "(" or ")")
                {
                    if (word is "{" or "(")
                        indentLevel++;

                    switch (indentLevel % 3)
                    {
                        case 1:
                            ChangeColor(ANSI_COLOR_YELLOW);
                            break;
                        case 2:
                            ChangeColor(ANSI_COLOR_PINK);
                            break;
                        default:
                            ChangeColor(ANSI_COLOR_BLUE);
                            break;
                    }

                    if (word is "}" or ")")
                        indentLevel--;
                }
                else if (word is ")" or "(")
                    ChangeColor(ANSI_COLOR_BLUE);
                else if (ReservedKeywords.Contains(lower) || Unreservedkeywords.Contains(lower) || BoolLiterals.Contains(lower))
                    ChangeColor(ANSI_COLOR_BLUE);
                else if (TypesBuiltIns.Contains(lower))
                    ChangeColor(ANSI_COLOR_GREEN);
                else if (ConstraintsBuiltIns.Contains(lower) || FunctionsBuiltIns.Contains(lower))
                    ChangeColor(ANSI_COLOR_YELLOW);
                else
                    ChangeColor(ANSI_COLOR_WHITE);

                coloredSchema += $"{word}";
            }

            return coloredSchema;
        }

        private static string[] ReservedKeywords = new string[]
        {
            "__source__",
            "__std__",
            "__subject__",
            "__type__",
            "abort",
            "alter",
            "analyze",
            "and",
            "anyarray",
            "anytuple",
            "anytype",
            "begin",
            "case",
            "check",
            "commit",
            "configure",
            "constraint",
            "create",
            "deallocate",
            "declare",
            "delete",
            "describe",
            "detached",
            "discard",
            "distinct",
            "do",
            "drop",
            "else",
            "empty",
            "end",
            "execute",
            "exists",
            "explain",
            "extending",
            "fetch",
            "filter",
            "for",
            "get",
            "global",
            "grant",
            "group",
            "if",
            "ilike",
            "import",
            "in",
            "insert",
            "introspect",
            "is",
            "like",
            "limit",
            "listen",
            "load",
            "lock",
            "match",
            "module",
            "move",
            "not",
            "notify",
            "offset",
            "optional",
            "or",
            "order",
            "over",
            "partition",
            "policy",
            "populate",
            "prepare",
            "raise",
            "refresh",
            "reindex",
            "release",
            "reset",
            "revoke",
            "rollback",
            "select",
            "set",
            "start",
            "typeof",
            "union",
            "update",
            "variadic",
            "when",
            "window",
            "with",
        };

        private static string[] Unreservedkeywords = new string[]
        {
            "abstract",
            "after",
            "alias",
            "all",
            "allow",
            "annotation",
            "as",
            "asc",
            "assignment",
            "before",
            "by",
            "cardinality",
            "cast",
            "config",
            "conflict",
            "constraint",
            "current",
            "database",
            "ddl",
            "default",
            "deferrable",
            "deferred",
            "delegated",
            "desc",
            "emit",
            "explicit",
            "expression",
            "final",
            "first",
            "from",
            "function",
            "implicit",
            "index",
            "infix",
            "inheritable",
            "into",
            "isolation",
            "json",
            "last",
            "link",
            "migration",
            "multi",
            "named",
            "object",
            "of",
            "oids",
            "on",
            "only",
            "onto",
            "operator",
            "overloaded",
            "owned",
            "postfix",
            "prefix",
            "property",
            "proposed",
            "pseudo",
            "read",
            "reject",
            "rename",
            "repeatable",
            "required",
            "restrict",
            "role",
            "roles",
            "savepoint",
            "scalar",
            "schema",
            "sdl",
            "serializable",
            "session",
            "single",
            "source",
            "superuser",
            "system",
            "target",
            "ternary",
            "text",
            "then",
            "to",
            "transaction",
            "type",
            "unless",
            "using",
            "verbose",
            "view",
            "write",
        };

        private static string[] BoolLiterals = new string[]
        {
            "false",
            "true",
        };

        private static string[] TypesBuiltIns = new string[]
        {
            "BaseObject",
            "Object",
            "anyenum",
            "anyfloat",
            "anyint",
            "anynumeric",
            "anyreal",
            "anyscalar",
            "array",
            "bigint",
            "bool",
            "bytes",
            "datetime",
            "decimal",
            "duration",
            "enum",
            "float32",
            "float64",
            "int16",
            "int32",
            "int64",
            "json",
            "local_date",
            "local_datetime",
            "local_time",
            "sequence",
            "str",
            "tuple",
            "uuid",
        };

        private static string[] ConstraintsBuiltIns = new string[]
        {
            "exclusive",
            "expression",
            "len_value",
            "max_ex_value",
            "max_len_value",
            "max_value",
            "min_ex_value",
            "min_len_value",
            "min_value",
            "one_of",
            "regexp",
        };

        private static string[] FunctionsBuiltIns = new string[]
        {
            "abs",
            "advisory_lock",
            "advisory_unlock",
            "advisory_unlock_all",
            "all",
            "any",
            "array_agg",
            "array_get",
            "array_join",
            "array_unpack",
            "bytes_get_bit",
            "ceil",
            "contains",
            "count",
            "date_get",
            "datetime_current",
            "datetime_get",
            "datetime_of_statement",
            "datetime_of_transaction",
            "datetime_truncate",
            "duration_to_seconds",
            "duration_truncate",
            "enumerate",
            "find",
            "floor",
            "get_current_database",
            "get_transaction_isolation",
            "get_version",
            "get_version_as_str",
            "json_array_unpack",
            "json_get",
            "json_object_unpack",
            "json_typeof",
            "len",
            "lg",
            "ln",
            "log",
            "max",
            "mean",
            "min",
            "random",
            "re_match",
            "re_match_all",
            "re_replace",
            "re_test",
            "round",
            "sleep",
            "stddev",
            "stddev_pop",
            "str_lower",
            "str_lpad",
            "str_ltrim",
            "str_pad_end",
            "str_pad_start",
            "str_repeat",
            "str_rpad",
            "str_rtrim",
            "str_split",
            "str_title",
            "str_trim",
            "str_trim_end",
            "str_trim_start",
            "str_upper",
            "sum",
            "time_get",
            "to_bigint",
            "to_datetime",
            "to_decimal",
            "to_duration",
            "to_float32",
            "to_float64",
            "to_int16",
            "to_int32",
            "to_int64",
            "to_json",
            "to_local_date",
            "to_local_datetime",
            "to_local_time",
            "to_str",
            "uuid_generate_v1mc",
            "var",
            "var_pop",
        };
    }
}
