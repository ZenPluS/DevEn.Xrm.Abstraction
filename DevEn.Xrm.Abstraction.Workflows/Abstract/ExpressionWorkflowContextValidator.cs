using System;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk.Workflow;
using DevEn.Xrm.Abstraction.Workflows.Core;

namespace DevEn.Xrm.Abstraction.Workflows.Abstract
{
    /// <summary>
    /// Base validator implementation wrapping an expression with lazy compilation and description.
    /// </summary>
    public abstract class ExpressionWorkflowContextValidator
        : IWorkflowContextValidator
    {
        private readonly Lazy<Func<IWorkflowContext, bool>> _compiled;
        public Expression<Func<IWorkflowContext, bool>> Expression { get; }
        public abstract string Description { get; }

        protected ExpressionWorkflowContextValidator(Expression<Func<IWorkflowContext, bool>> expr)
        {
            Expression = expr ?? throw new ArgumentNullException(nameof(expr));
            _compiled = new Lazy<Func<IWorkflowContext, bool>>(() => Expression.Compile());
        }

        public bool IsValid(IWorkflowContext context)
            => _compiled.Value(context);

        public override string ToString() => Description;
    }
}
