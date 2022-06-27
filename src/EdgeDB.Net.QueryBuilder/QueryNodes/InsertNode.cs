﻿using EdgeDB.Serializer;
using System;
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
        public InsertNode(NodeBuilder builder) : base(builder) 
        {
            _children = new();
        }

        private string BuildInsertLambdaShape(LambdaExpression expression)
        {
            return $"{{ {ExpressionTranslator.Translate(expression, Builder.QueryVariables, Context)} }}";
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
                var propertyName = property.GetEdgeDBPropertyName();
                
                var edgeqlType = PacketSerializer.GetEdgeQLType(property.PropertyType);

                if(edgeqlType != null)
                {
                    var varName = QueryUtils.GenerateRandomVariableName();
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
                        var globalName = QueryUtils.GenerateRandomVariableName();
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
                            var globalName = QueryUtils.GenerateRandomVariableName();
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
        
        public override void Visit()
        {
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
                SetGlobal(Context.GlobalName, new SubQuery($"({Query})"));
                Query.Clear();
            }
        }

        public void UnlessConflict()
        {
            _autogenerateUnlessConflict = true;
        }

        public void UnlessConflictOn(LambdaExpression selector)
        {
            Query.Append($" unless conflict on {ExpressionTranslator.Translate(selector, Builder.QueryVariables, Context)}");
        }

        public void ElseDefault()
        {
            _children.Append($" else (select {Context.CurrentType.GetEdgeDBTypeName()})");
        }

        public void Else(IQueryBuilder builder)
        {
            var userNodes = builder.Nodes.Where(x => !x.IsAutoGenerated);

            // TODO: better checks for this, future should add a callback to add the
            // node with its context so any parent builder can change contexts for nodes
            foreach (var node in userNodes)
                node.Context.SetAsGlobal = false;

            var globals = userNodes.SelectMany(x =>
                x.ReferencedGlobals.Select(y =>
                    new KeyValuePair<string, object?>(y, x.Builder.QueryGlobals[y])
                )
            ).ToDictionary(x => x.Key, x => x.Value);

            var variables = userNodes.SelectMany(x =>
                x.ReferencedVariables.Select(y =>
                    new KeyValuePair<string, object?>(y, x.Builder.QueryVariables[y])
                )
            );


            var newBuilder = new QueryBuilder<object?>(userNodes.ToList(), globals);

            var result = newBuilder.BuildWithGlobals();
            _children.Append($" else ({result.Query})");

            foreach (var variable in variables)
                SetVariable(variable.Key, variable.Value);
            foreach (var global in globals)
                SetGlobal(global.Key, global.Value);
        }
    }
}
