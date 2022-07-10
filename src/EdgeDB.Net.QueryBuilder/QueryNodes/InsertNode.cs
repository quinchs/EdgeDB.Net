using EdgeDB.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class InsertNode : QueryNode<InsertContext>
    {
        private bool _autogenerateUnlessConflict;
        private readonly StringBuilder _children;
        private readonly List<Type> _subQueryMap = new();
        
        public InsertNode(NodeBuilder builder) : base(builder) 
        {
            _children = new();
        }

        private string BuildInsertLambdaShape(LambdaExpression expression)
        {
            return $"{{ {ExpressionTranslator.Translate(expression, Builder.QueryVariables, Context, Builder.QueryGlobals)} }}";
        }
        
        private string BuildInsertShape(Type? shapeType = null, object? shapeValue = null)
        {
            List<string> shape = new();
            
            var type = shapeType ?? Context.CurrentType;
            var value = shapeValue ?? Context.Value;

            if (value is LambdaExpression expression)
                return BuildInsertLambdaShape(expression);

            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);

            foreach(var property in properties)
            {
                // might be multi link
                var propType = property.PropertyType;
                var isLink = QueryUtils.IsLink(property.PropertyType, out var isArray, out var innerType);
                
                var propertyName = property.GetEdgeDBPropertyName();
                
                var edgeqlType = PacketSerializer.GetEdgeQLType(propType);

                if(edgeqlType != null)
                {
                    var varName = QueryUtils.GenerateRandomVariableName();
                    SetVariable(varName, property.GetValue(value));
                    shape.Add($"{propertyName} := <{edgeqlType}>${varName}");
                    continue;
                }

                // might be a link?
                if (isLink)
                {
                    var subValue = property.GetValue(value);

                    if (subValue is null)
                        shape.Add($"{propertyName} := {{}}");
                    else if (isArray)
                    {
                        List<string> subShape = new();
                        foreach (var item in (IEnumerable)subValue!)
                        {
                            subShape.Add(BuildLinkResolver(innerType!, item));
                        }

                        shape.Add($"{propertyName} := {{ {string.Join(", ", subShape)} }}");
                    }
                    else
                        shape.Add($"{propertyName} := {BuildLinkResolver(propType, subValue)}");

                    continue;
                }

                throw new Exception($"Failed to find method to serialize the property \"{property.PropertyType.Name}\" on type {type.Name}");
            }

            return $"{{ {string.Join(", ", shape)} }}";
        }

        private string BuildLinkResolver(Type type, object? value)
        {
            if (value is null)
                return "{}";

            if (QueryObjectManager.TryGetObjectId(value, out var id))
            {
                return InlineOrGlobal(
                    type,
                    new SubQuery($"(select {type.GetEdgeDBTypeName()} filter .id = <uuid>\"{id}\")"),
                    value);
            }
            else
            {
                RequiresIntrospection = true;
                return InlineOrGlobal(type, new SubQuery((info) =>
                {
                    var name = type.GetEdgeDBTypeName();
                    var exclusiveProps = QueryUtils.GetProperties(info, type, true);
                    var exclusiveCondition = exclusiveProps.Any() ?
                        $" unless conflict on {(exclusiveProps.Count() > 1 ? $"({string.Join(", ", exclusiveProps.Select(x => $".{x.GetEdgeDBPropertyName()}"))})" : $".{exclusiveProps.First().GetEdgeDBPropertyName()}")} else (select {name})"
                        : string.Empty;
                    return $"(insert {name} {BuildInsertShape(type, value)}{exclusiveCondition})";
                }), value);
            }
        }

        private string InlineOrGlobal(Type type, object value, object? reference)
        {
            if (_subQueryMap.Contains(type) || (value is SubQuery sq && sq.RequiresIntrospection))
                return GetOrAddGlobal(reference, value);

            _subQueryMap.Add(type);
            return value is SubQuery subQuery && subQuery.Query != null
                ? subQuery.Query
                : value.ToString()!;
        }

        public override void Visit()
        {
            _subQueryMap.Add(Context.CurrentType);
            var shape = BuildInsertShape();
            Query.Append($"insert {Context.CurrentType.GetEdgeDBTypeName()} {shape}");
        }

        public override void FinalizeQuery()
        {
            if(_autogenerateUnlessConflict)
            {
                if (SchemaInfo is null)
                    throw new NotSupportedException("Cannot use autogenerated unless conflict on without schema interpolation");

                if (!SchemaInfo.TryGetObjectInfo(Context.CurrentType, out var typeInfo))
                    throw new NotSupportedException($"Could not find type info for {Context.CurrentType}");

                var exclusiveProperties = typeInfo.Properties?.Where(x => x.IsExclusive && x.Name != "id");

                if (exclusiveProperties == null || !exclusiveProperties.Any())
                    throw new NotSupportedException($"The type {typeInfo.Name} does not have any user defined exclusive properties");

                var constraint = exclusiveProperties.Count() > 1 ?
                    $"({string.Join(", ", exclusiveProperties.Select(x => $".{x.Name}"))})" :
                    $".{exclusiveProperties.First().Name}";

                Query.Append($" unless conflict on {constraint}");
            }

            Query.Append(_children);
            
            if(Context.SetAsGlobal && Context.GlobalName != null)
            {
                SetGlobal(Context.GlobalName, new SubQuery($"({Query})"), null);
                Query.Clear();
            }
        }

        public void UnlessConflict()
        {
            _autogenerateUnlessConflict = true;
            RequiresIntrospection = true;
        }

        public void UnlessConflictOn(LambdaExpression selector)
        {
            Query.Append($" unless conflict on {ExpressionTranslator.Translate(selector, Builder.QueryVariables, Context, Builder.QueryGlobals)}");
        }

        public void ElseDefault()
        {
            _children.Append($" else (select {Context.CurrentType.GetEdgeDBTypeName()})");
        }

        public void Else(IQueryBuilder builder)
        {
            // remove addon & autogen nodes.
            var userNodes = builder.Nodes.Where(x => !builder.Nodes.Any(y => y.SubNodes.Contains(x)) || !x.IsAutoGenerated);

            // TODO: better checks for this, future should add a callback to add the
            // node with its context so any parent builder can change contexts for nodes
            foreach (var node in userNodes)
                node.Context.SetAsGlobal = false;

            foreach(var variable in builder.Variables)
            {
                Builder.QueryVariables[variable.Key] = variable.Value;
            }

            var newBuilder = new QueryBuilder<object?>(userNodes.ToList(), builder.Globals.ToList(), builder.Variables.ToDictionary(x => x.Key, x=> x.Value));

            var result = newBuilder.BuildWithGlobals();
            _children.Append($" else ({result.Query})");
        }
    }
}
