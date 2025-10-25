using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs
{
    public class FakePluginExecutionContext
        : IPluginExecutionContext
    {
        // Only properties actually used by the plugins/tests are fully implemented.
        public string MessageName { get; set; }
        public string PrimaryEntityName { get; set; }
        public Guid UserId { get; set; }
        public Guid InitiatingUserId { get; set; }
        public ParameterCollection InputParameters { get; } = new ParameterCollection();
        public ParameterCollection OutputParameters { get; } = new ParameterCollection();
        public EntityImageCollection PreEntityImages { get; } = new EntityImageCollection();
        public EntityImageCollection PostEntityImages { get; } = new EntityImageCollection();
        public ParameterCollection SharedVariables { get; } = new ParameterCollection();

        // Unused members return defaults / throw if accessed unexpectedly.
        public int Stage { get; set; }
        public int Mode { get; set; }
        public int IsolationMode { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid OperationId { get; set; }
        public DateTime OperationCreatedOn { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid PrimaryEntityId { get; set; }
        public Guid RequestId { get; set; }
        public string SecondaryEntityName { get; set; }
        public bool IsExecutingOffline { get; set; }
        public bool IsOfflinePlayback { get; set; }
        public bool IsInTransaction { get; set; }
        public bool IsClone { get; set; }
        public int Depth { get; set; }
        public string ParentContextId { get; set; }
        public Guid BusinessUnitId { get; set; }
        public Guid OwningExtensionId { get; set; }
        public string OwningExtensionLogicalName { get; set; }
        public string OwningExtensionUniqueName { get; set; }
        public Guid InitiatingUserAzureActiveDirectoryObjectId { get; set; }
        public Guid UserAzureActiveDirectoryObjectId { get; set; }
        public bool IsDataEncryptionEnabled { get; set; }
        public int? ProcessingStepPosition { get; set; }

        public IEnumerable<IPluginExecutionContext> ParentContext => new List<IPluginExecutionContext>();
        public string StageName { get; set; }
        public string ModeName { get; set; }

        public int? SharedVariableKeyIndex { get; set; }

        public string MessageCategory { get; set; }
        public string MessageCategoryName { get; set; }
        public string MessageId { get; set; }

        public Guid OperationParentId { get; set; }

        public int MaxDepth { get; set; }

        public EntityReference OwningExtension { get; set; }

        IPluginExecutionContext IPluginExecutionContext.ParentContext { get; }

        Guid? IExecutionContext.RequestId => RequestId;
    }
}