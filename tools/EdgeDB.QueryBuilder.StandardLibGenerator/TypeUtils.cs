using EdgeDB.DataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryBuilder.StandardLibGenerator
{
    public class TypeUtils
    {
        public static Type GetType(string t)
        {
            return t switch
            {
                "std::anytype" => typeof(object),
                "std::set" => typeof(IEnumerable),
                "std::anytuple" => typeof(ITuple),
                "std::anyenum" => typeof(Enum),
                "std::Object" => typeof(object),
                "std::bool" => typeof(bool),
                "std::bytes" => typeof(byte[]),
                "std::str" => typeof(string),
                "cal::local_date" => typeof(DateOnly),
                "cal::local_time" => typeof(TimeSpan),
                "cal::local_datetime" => typeof(DateTime),
                "cal::relative_duration" => typeof(TimeSpan),
                "std::datetime" => typeof(DateTimeOffset),
                "std::duration" => typeof(TimeSpan),
                "std::float32" => typeof(float),
                "std::float64" => typeof(double),
                "std::int8" => typeof(sbyte),
                "std::int16" => typeof(short),
                "std::int32" => typeof(int),
                "std::int64" => typeof(long),
                "std::bigint" => typeof(BigInteger),
                "std::decimal" => typeof(decimal),
                "std::uuid" => typeof(Guid),
                "std::json" => typeof(Json),
                _ => throw new Exception($"Type {t} not found")
            };
        }
    }
}
