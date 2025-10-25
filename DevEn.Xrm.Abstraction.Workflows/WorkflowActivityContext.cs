using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using DevEn.Xrm.Abstraction.Workflows.Core;
using System.Activities;

namespace DevEn.Xrm.Abstraction.Workflows
{
    public class WorkflowActivityContext : IWorkflowActivityContext
    {
        public IWorkflowContext WorkflowContext { get; }
        public IOrganizationService UserOrganizationService { get; }
        public IOrganizationService SystemOrganizationService { get; }
        public ITracingService TracingService { get; }

        public WorkflowActivityContext(CodeActivityContext codeContext)
        {
            if (codeContext == null) throw new ArgumentNullException(nameof(codeContext));

            WorkflowContext = codeContext.GetExtension<IWorkflowContext>();
            var factory = codeContext.GetExtension<IOrganizationServiceFactory>();
            TracingService = codeContext.GetExtension<ITracingService>();

            UserOrganizationService = factory.CreateOrganizationService(WorkflowContext.UserId);
            SystemOrganizationService = factory.CreateOrganizationService(WorkflowContext.InitiatingUserId);
        }

        public T GetInput<T>(InArgument<T> argument, CodeActivityContext ctx)
            => argument == null ? default : argument.Get(ctx);

        public void SetOutput<T>(OutArgument<T> argument, CodeActivityContext ctx, T value)
        {
            if (argument != null)
                argument.Set(ctx, value);
        }
    }
}