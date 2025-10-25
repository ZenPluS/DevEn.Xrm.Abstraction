using Microsoft.Xrm.Sdk;
using System;
using System.Linq.Expressions;

namespace DevEn.Xrm.Abstraction.Plugins.Core
{
    public interface IBasePlugin
        : IPlugin
    {
        Expression<Func<IPluginExecutionContext, bool>> ValidationExpression { get; }
        string Header { get; }
        void ExecutePlugin(IPluginContext context);
        bool IsContextValid();
    }
}
