using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins.Core
{
    public interface IPluginContext
    {
        IPluginExecutionContext Context { get; }
        IOrganizationService UserOrganizationService { get; }
        IOrganizationService SystemOrganizationService { get; }
        ITracingService TracingService { get; }
        T GetInputParameter<T>(string name);
        T GetOutputParameter<T>(string name);
        void SetOutputParameter(string name, object value);
        Entity GetTargetEntity();
        Entity GetPreImage(string name);
        Entity GetPostImage(string name);
        T GetSharedVariable<T>(string name);
    }
}
