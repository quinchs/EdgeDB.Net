using EdgeDB.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class InsertNode : QueryNode<InsertContext>
    {
        private const string VARIABLE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly Random _rng = new();

        public override bool IsRootNode => true;

        public override QueryExpressionType? ValidChildren 
            => QueryExpressionType.UnlessConflictOn | QueryExpressionType.Else;

        public InsertNode(QueryBuilder builder) : base(builder) { }

        private string BuildInsertShape(Type? shapeType = null, object? shapeValue = null)
        {
            List<string> shape = new();
            
            var type = shapeType ?? Context.CurrentType;
            var value = shapeValue ?? Context.Value;

            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);

            foreach(var property in properties)
            {
                var propertyName = property.GetEdgeDBPropertyName();
                
                var edgeqlType = PacketSerializer.GetEdgeQLType(property.PropertyType);

                if(edgeqlType != null)
                {
                    var varName = GenerateRandomVariableName();
                    SetVariable(varName, property.GetValue(Context.Value));
                    shape.Add($"{propertyName} := <{edgeqlType}>${varName}");
                    continue;
                }

                // TODO: sub queries!
                // might be a link?
                if (TypeBuilder.IsValidObjectType(property.PropertyType))
                {
                    // is it a object we've seen before?
                    var subValue = property.GetValue(value);
                    if(QueryObjectManager.TryGetObjectId(subValue, out var id))
                    {
                        // insert a sub query
                        var globalName = GenerateRandomVariableName();
                        SetGlobal(globalName, new SubQuery($"(select {property.PropertyType.GetEdgeDBTypeName()} filter .id = <uuid>\"{id}\")"));
                        shape.Add($"{propertyName} := {globalName}");
                        continue;
                    }
                    else
                    {
                        if (subValue is null)
                            shape.Add($"{propertyName} := {{}}");
                        else
                        {
                            var globalName = GenerateRandomVariableName();
                            SetGlobal(globalName, new SubQuery($"(insert {property.PropertyType.GetEdgeDBTypeName()} {BuildInsertShape(property.PropertyType, subValue)})"));
                            shape.Add($"{propertyName} := {globalName}");
                        }
                        
                        continue;
                    }
                }

                throw new Exception($"Failed to find method to serialize the property \"{property.PropertyType.Name}\" on type {type.Name}");
            }

            return $"{{ {string.Join(", ", shape)} }}";
        }

        private static string GenerateRandomVariableName()
        {
            return new string(Enumerable.Repeat(VARIABLE_CHARS, 12).Select(x => x[_rng.Next(x.Length)]).ToArray());
        }

        public override void Visit()
        {
            var shape = BuildInsertShape();
            var insert = $"insert {Context.CurrentType.GetEdgeDBTypeName()} {shape}";

            if (Context.StoreAsGlobal)
            {
                var globalName = GenerateRandomVariableName();
                SetGlobal(globalName, new SubQuery(insert));
                Context.GlobalName = globalName;
            }
            else
                Query.Append(insert);
        }
    }
}
