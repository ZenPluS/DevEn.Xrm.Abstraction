using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;

namespace DevEn.Xrm.Abstraction.Workflows.UnitTests.Stubs
{
    public class FakeWorkflowContext : IWorkflowContext
    {
        public ParameterCollection SharedVariables { get; }
        public Guid UserId { get; set; }
        public Guid InitiatingUserId { get; set; }
        public Guid BusinessUnitId { get; }
        public string PrimaryEntityName { get; set; }
        Guid? IExecutionContext.RequestId { get; }
        public Guid PrimaryEntityId { get; set; }
        public EntityImageCollection PreEntityImages { get; }
        public EntityImageCollection PostEntityImages { get; }
        public EntityReference OwningExtension { get; }
        public string MessageName { get; set; }

        // Unused members minimal / defaults
        public Guid CorrelationId { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid RequestId { get; set; }
        public int Stage { get; set; }
        public int Mode { get; set; }
        public int IsolationMode { get; }
        public bool IsExecutingOffline { get; set; }
        public bool IsOfflinePlayback { get; set; }
        public bool IsInTransaction { get; set; }
        public Guid OperationId { get; set; }
        public DateTime OperationCreatedOn { get; set; }
        public string SecondaryEntityName { get; set; }
        public ParameterCollection InputParameters { get; }
        public ParameterCollection OutputParameters { get; }
        public int Depth { get; set; }
        public string ParentContextId { get; set; }
        public Guid OwningExtensionId { get; set; }
        public string OwningExtensionLogicalName { get; set; }
        public string OwningExtensionUniqueName { get; set; }
        public string StageName { get; }
        public int WorkflowCategory { get; }
        public int WorkflowMode { get; }
        public IWorkflowContext ParentContext { get; }
    }
}