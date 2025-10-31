using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows.Core
{
    /// <summary>
    /// Abstraction providing access to workflow execution context, tracing and organization services,
    /// plus helpers to read input and assign output arguments.
    /// </summary>
    public interface IWorkflowActivityContext
    {
        /// <summary>Underlying Dataverse workflow execution context.</summary>
        IWorkflowContext WorkflowContext { get; }

        /// <summary>Organization service scoped to the current user.</summary>
        IOrganizationService UserOrganizationService { get; }

        /// <summary>Organization service scoped to the initiating (system) user.</summary>
        IOrganizationService SystemOrganizationService { get; }

        /// <summary>Tracing service used for diagnostic logging.</summary>
        ITracingService TracingService { get; }

        /// <summary>Factory used to create organization services.</summary>
        IOrganizationServiceFactory ServiceFactory { get; }

        /// <summary>
        /// Retrieves an input argument value from the workflow execution context.
        /// Returns default if argument is null.
        /// </summary>
        /// <typeparam name="T">Argument value type.</typeparam>
        /// <param name="argument">InArgument to read.</param>
        /// <param name="ctx">WF execution context.</param>
        T GetInput<T>(InArgument<T> argument, CodeActivityContext ctx);

        /// <summary>
        /// Assigns a value to an output argument in the workflow execution context if the argument is not null.
        /// </summary>
        /// <typeparam name="T">Argument value type.</typeparam>
        /// <param name="argument">OutArgument to set.</param>
        /// <param name="ctx">WF execution context.</param>
        /// <param name="value">Value to assign.</param>
        void SetOutput<T>(OutArgument<T> argument, CodeActivityContext ctx, T value);
    }
}