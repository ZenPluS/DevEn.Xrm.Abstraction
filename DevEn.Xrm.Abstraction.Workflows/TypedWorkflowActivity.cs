using System;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using DevEn.Xrm.Abstraction.Workflows.Core;

namespace DevEn.Xrm.Abstraction.Workflows
{
    /// <summary>
    /// Generic base class for single-input / single-output workflow activities offering strongly typed execution.
    /// Handles:
    /// - Input retrieval
    /// - Core execution via <see cref="ExecuteCore"/>
    /// - Output assignment
    /// - Exception wrapping as <see cref="InvalidWorkflowException"/>
    /// Validation is still performed by the parent <see cref="BaseWorkflowActivity"/>.
    /// </summary>
    /// <typeparam name="TIn">Type of the single input argument.</typeparam>
    /// <typeparam name="TOut">Type of the single output argument.</typeparam>
    public abstract class TypedWorkflowActivity<TIn, TOut> : BaseWorkflowActivity
    {
        [RequiredArgument]
        [Input("Input")]
        public InArgument<TIn> Input { get; set; }

        [Output("Result")]
        public OutArgument<TOut> Result { get; set; }

        /// <summary>
        /// Orchestrates typed execution. If validation (in base class) skips execution, the framework still
        /// exposes the declared OutArgument key in the results dictionary with a default (null) value.
        /// </summary>
        protected sealed override void ExecuteWorkflow(IWorkflowActivityContext context, CodeActivityContext executionContext)
        {
            var tracing = context.TracingService;
            var inputValue = context.GetInput(Input, executionContext);
            try
            {
                var output = ExecuteCore(context, inputValue);
                context.SetOutput(Result, executionContext, output);
            }
            catch (Exception ex)
            {
                tracing.Trace($"{Header} - Core execution exception: {ex.Message}");
                throw new InvalidWorkflowException($"Error in {Header}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Implement business logic returning the typed output.
        /// </summary>
        protected abstract TOut ExecuteCore(IWorkflowActivityContext ctx, TIn input);
    }
}