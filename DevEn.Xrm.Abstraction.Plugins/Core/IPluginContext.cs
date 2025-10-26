using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins.Core
{
    /// <summary>
    /// Abstraction for plugin execution context providing access to services, parameters, images, and shared variables.
    /// </summary>
    public interface IPluginContext
    {
        /// <summary>Underlying pipeline execution context.</summary>
        IPluginExecutionContext Context { get; }
        /// <summary>User-scoped organization service.</summary>
        IOrganizationService UserOrganizationService { get; }
        /// <summary>Initiating user (system) organization service.</summary>
        IOrganizationService SystemOrganizationService { get; }
        /// <summary>Tracing service for diagnostic messages.</summary>
        ITracingService TracingService { get; }
        /// <summary>Gets an input parameter value by name or default.</summary>
        T GetInputParameter<T>(string name);
        /// <summary>Gets an output parameter value by name or default.</summary>
        T GetOutputParameter<T>(string name);
        /// <summary>Sets an output parameter value by name.</summary>
        void SetOutputParameter(string name, object value);
        /// <summary>Returns the Target entity if present (Entity or converted from EntityReference).</summary>
        Entity GetTargetEntity();
        /// <summary>Gets a pre-image entity by image name.</summary>
        Entity GetPreImage(string name);
        /// <summary>Gets a post-image entity by image name.</summary>
        Entity GetPostImage(string name);
        /// <summary>Gets a shared variable by key or default if absent.</summary>
        T GetSharedVariable<T>(string name);
    }
}
