using System;
using System.Activities;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Workflows.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows
{
    public abstract class BaseWorkflowActivity : CodeActivity
    {
        protected virtual string Header => GetType().Name;
        protected abstract Expression<Func<IWorkflowContext, bool>> ValidationExpression { get; }

        protected abstract void ExecuteWorkflow(IWorkflowActivityContext context, CodeActivityContext executionContext);

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

    public class InvalidWorkflowException : Exception
    {
        public InvalidWorkflowException(string message) : base(message) { }

        public InvalidWorkflowException() : base()
        {
        }

        public InvalidWorkflowException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}