using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DevEn.Xrm.Abstraction.Plugins.CustomAttributes;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins
{
    /// <summary>
    /// Plugin base that derives its validation logic dynamically from one or more <see cref="PluginFilterAttribute"/> instances.
    /// Each attribute contributes a (Message AND Optional Entity) clause; clauses are OR-composed.
    /// If no attributes are present, validation returns null (treated as always valid).
    /// </summary>
    public abstract class FilteredPluginBase : BasePlugin
    {
        private readonly Lazy<Expression<Func<IPluginExecutionContext, bool>>> _expr;

        protected FilteredPluginBase()
        {
            _expr = new Lazy<Expression<Func<IPluginExecutionContext, bool>>>(BuildExpression);
        }

        public override Expression<Func<IPluginExecutionContext, bool>> ValidationExpression
            => _expr.Value;

        private static PropertyInfo GetInterfacePropertyRecursive(Type type, string name)
        {
            var p = type.GetProperty(name);
            if (p != null)
                return p;

            if (!type.IsInterface)
                return null;
            foreach (var i in type.GetInterfaces())
            {
                p = GetInterfacePropertyRecursive(i, name);
                if (p != null) return p;
            }
            return null;
        }

        private Expression<Func<IPluginExecutionContext, bool>> BuildExpression()
        {
            var filters = GetType()
                .GetCustomAttributes(typeof(PluginFilterAttribute), true)
                .Cast<PluginFilterAttribute>()
                .ToList();

            if (filters.Count == 0)
                return null;

            var ctxParam = Expression.Parameter(typeof(IPluginExecutionContext), "ctx");
            Expression body = null;

            var messagePropInfo = GetInterfacePropertyRecursive(typeof(IPluginExecutionContext), nameof(IPluginExecutionContext.MessageName));
            var entityPropInfo = GetInterfacePropertyRecursive(typeof(IPluginExecutionContext), nameof(IPluginExecutionContext.PrimaryEntityName));

            foreach (var f in filters)
            {
                if (messagePropInfo == null)
                    throw new InvalidPluginExecutionException($"Cannot locate property '{nameof(IPluginExecutionContext.MessageName)}' on IPluginExecutionContext inheritance chain.");

                var msgProp = Expression.Property(ctxParam, messagePropInfo);
                var msgEq = Expression.Call(
                    msgProp,
                    typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) }) ?? throw new InvalidOperationException(),
                    Expression.Constant(f.Message),
                    Expression.Constant(StringComparison.OrdinalIgnoreCase)
                );

                Expression entityCheck = Expression.Constant(true);
                if (!string.IsNullOrEmpty(f.EntityLogicalName))
                {
                    if (entityPropInfo == null)
                        throw new InvalidPluginExecutionException($"Cannot locate property '{nameof(IPluginExecutionContext.PrimaryEntityName)}' on IPluginExecutionContext inheritance chain.");

                    var entProp = Expression.Property(ctxParam, entityPropInfo);
                    entityCheck = Expression.Call(
                        entProp,
                        typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) }) ?? throw new InvalidOperationException(),
                        Expression.Constant(f.EntityLogicalName),
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                    );
                }

                var and = Expression.AndAlso(msgEq, entityCheck);
                body = body == null ? and : Expression.OrElse(body, and);
            }

            return Expression.Lambda<Func<IPluginExecutionContext, bool>>(body ?? Expression.Empty(), ctxParam);
        }
    }
}