using System;
using System.Activities;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Workflows.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows
{
    /// <summary>
    /// Foundation workflow activity implementing the common execution pipeline:
    /// 1. Wrap execution context
    /// 2. Trace start
    /// 3. Validate (expression or validator object)
    /// 4. Invoke derived <see cref="ExecuteWorkflow"/> if valid
    /// 5. Catch and wrap errors as <see cref="InvalidWorkflowException"/>
    /// Derived classes provide either a <see cref="ValidationExpression"/> or an <see cref="IWorkflowContextValidator"/> plus the core logic.
    /// </summary>
    public abstract class BaseWorkflowActivity
        : CodeActivity
    {
        /// <summary>Short label used in tracing and error wrapping. Defaults to concrete type name.</summary>
        protected virtual string Header => GetType().Name;

        /// <summary>Optional validator abstraction; if supplied its expression supersedes <see cref="ValidationExpression"/>.</summary>
        protected virtual IWorkflowContextValidator Validator => null;

        /// <summary>
        /// Validation predicate executed (compiled) against <see cref="IWorkflowContext"/> when no explicit validator instance is provided.
        /// Return true to continue execution; false skips.
        /// </summary>
        protected virtual Expression<Func<IWorkflowContext, bool>> ValidationExpression => Validator?.Expression;

        /// <summary>Derived classes implement their business logic here (only invoked when validation passes).</summary>
        protected abstract void ExecuteWorkflow(IWorkflowActivityContext context, CodeActivityContext executionContext);

        /// <summary>
        /// Orchestrates validation, execution and exception handling.
        /// </summary>
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
        /// Performs safe validation using either <see cref="Validator"/> or compiled <see cref="ValidationExpression"/>.
        /// Any exception during validation is traced and treated as a failure (skip).
        /// </summary>
        protected virtual bool IsContextValid(IWorkflowContext ctx, ITracingService tracing)
        {
            if (Validator != null)
                return SafeEval(() => Validator.IsValid(ctx), Validator.Description, tracing);

            return ValidationExpression == null || SafeEval(() => ValidationExpression.Compile()(ctx), "ValidationExpression", tracing);
        }

        private bool SafeEval(Func<bool> eval, string label, ITracingService tracing)
        {
            try { return eval(); }
            catch (Exception ex)
            {
                tracing.Trace($"{Header} - {label} exception: {ex.Message}");
                return false;
            }
        }
    }
}