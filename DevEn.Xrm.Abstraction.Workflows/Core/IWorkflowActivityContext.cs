using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows.Core
{
    public interface IWorkflowActivityContext
    {
        IWorkflowContext WorkflowContext { get; }
        IOrganizationService UserOrganizationService { get; }
        IOrganizationService SystemOrganizationService { get; }
        ITracingService TracingService { get; }
        T GetInput<T>(System.Activities.InArgument<T> argument, System.Activities.CodeActivityContext ctx);
        void SetOutput<T>(System.Activities.OutArgument<T> argument, System.Activities.CodeActivityContext ctx, T value);
    }
}