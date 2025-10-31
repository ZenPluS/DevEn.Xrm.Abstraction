using System;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;
using DevEn.Xrm.Abstraction.Plugins.Core;

namespace DevEn.Xrm.Abstraction.Plugins.Abstract
{
    /// <summary>
    /// Base validator implementation wrapping an expression with lazy compilation and description for plugin contexts.
    /// </summary>
    public abstract class ExpressionPluginContextValidator
        : IPluginContextValidator
    {
        private readonly Lazy<Func<IPluginExecutionContext, bool>> _compiled;
        public Expression<Func<IPluginExecutionContext, bool>> Expression { get; }
        public abstract string Description { get; }

        protected ExpressionPluginContextValidator(Expression<Func<IPluginExecutionContext, bool>> expr)
        {
            Expression = expr ?? throw new ArgumentNullException(nameof(expr));
            _compiled = new Lazy<Func<IPluginExecutionContext, bool>>(() => Expression.Compile());
        }

        public bool IsValid(IPluginExecutionContext context)
            => _compiled.Value(context);

        public override string ToString() => Description;
    }
}
