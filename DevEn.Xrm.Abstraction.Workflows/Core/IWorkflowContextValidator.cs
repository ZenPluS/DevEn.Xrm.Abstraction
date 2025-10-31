using System;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows.Core
{
    /// <summary>
    /// Contract for workflow context validation providing an expression and executable predicate.
    /// </summary>
    public interface IWorkflowContextValidator
    {
        /// <summary>
        /// Gets the description associated with the current instance.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the predicate expression used to evaluate whether a workflow context satisfies specific conditions.
        /// </summary>
        /// <remarks>The returned expression can be compiled and invoked to determine if an instance of
        /// <see cref="IWorkflowContext"/> meets the criteria defined by the predicate. This property is typically used
        /// for filtering or conditional logic within workflow operations.</remarks>
        Expression<Func<IWorkflowContext, bool>> Expression { get; }

        /// <summary>
        /// Determines whether the specified workflow context meets the criteria for validity.
        /// </summary>
        /// <param name="context">The workflow context to evaluate for validity. Cannot be null.</param>
        /// <returns>true if the workflow context is valid; otherwise, false.</returns>
        bool IsValid(IWorkflowContext context);
    }
}
