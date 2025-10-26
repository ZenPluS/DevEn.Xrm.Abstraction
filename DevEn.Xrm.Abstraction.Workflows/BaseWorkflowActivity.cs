using System;
using System.Activities;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Workflows.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows
{
    /// <summary>
    /// Base abstract workflow activity providing standardized execution flow:
    /// tracing start, validating context via <see cref="ValidationExpression"/>, invoking derived logic and handling exceptions.
    /// Derive and implement <see cref="ValidationExpression"/> and <see cref="ExecuteWorkflow(IWorkflowActivityContext, CodeActivityContext)"/>.
    /// </summary>
    public abstract class BaseWorkflowActivity
        : CodeActivity
    {
        /// <summary>
        /// Header used in trace and exception messages. Defaults to the concrete type name.
        /// </summary>
        protected virtual string Header => GetType().Name;

        /// <summary>
        /// Expression used to validate whether the current <see cref="IWorkflowContext"/> should execute this activity.
        /// Must return true to proceed; false skips execution.
        /// </summary>
        protected abstract Expression<Func<IWorkflowContext, bool>> ValidationExpression { get; }

        /// <summary>
        /// Core business logic for the workflow activity. Implement this in derived classes.
        /// </summary>
        /// <param name="context">Wrapper exposing workflow services and context.</param>
        /// <param name="executionContext">Underlying WF execution context.</param>
        protected abstract void ExecuteWorkflow(IWorkflowActivityContext context, CodeActivityContext executionContext);

        /// <summary>
        /// Workflow runtime entry point. Wraps the execution context, traces start, validates, and calls <see cref="ExecuteWorkflow"/>.
        /// Handles and rethrows exceptions as <see cref="InvalidWorkflowException"/>.
        /// </summary>
        /// <param name="executionContext">WF execution context provided by runtime.</param>
        protected override void Execute(CodeActivityContext executionContext)
        {
            var wrapper = new WorkflowActivityContext(executionContext);
            var tracing = wrapper.TracingService;
            var wfCtx = wrapper.WorkflowContext;

            tracing.Trace($"{Header} - Start Execute. Message: {wfCtx.MessageName}, Entity: {wfCtx.PrimaryEntityName}");

            try
            {
                if (!IsContextValid(wfCtx, tracing))
                {
                    tracing.Trace($"Context not valid for workflow {Header}. Skipping.");
                    return;
                }

                ExecuteWorkflow(wrapper, executionContext);
            }
            catch (Exception ex)
            {
                tracing.Trace($"{Header} - Exception: {ex.Message}");
                throw new InvalidWorkflowException($"Error in {Header}: {ex.Message}");
            }
        }

        /// <summary>
        /// Evaluates <see cref="ValidationExpression"/> against the current workflow context.
        /// Returns true if execution should continue; false otherwise. Traces validation errors.
        /// </summary>
        /// <param name="ctx">Workflow context.</param>
        /// <param name="tracing">Tracing service for diagnostics.</param>
        /// <returns>True if valid; false if not.</returns>
        protected virtual bool IsContextValid(IWorkflowContext ctx, ITracingService tracing)
        {
            if (ValidationExpression == null)
                return true;

            try
            {
                return ValidationExpression.Compile()(ctx);
            }
            catch (Exception ex)
            {
                tracing.Trace($"{Header} - ValidationExpression exception: {ex.Message}");
                return false;
            }
        }
    }
}