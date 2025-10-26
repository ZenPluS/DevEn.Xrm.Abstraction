using Microsoft.Xrm.Sdk;
using System;
using System.Linq.Expressions;

namespace DevEn.Xrm.Abstraction.Plugins.Core
{
    /// <summary>
    /// Contract for base plugin abstraction providing validation, header, execution, and context validity check.
    /// </summary>
    public interface IBasePlugin
        : IPlugin
    {
        /// <summary>Expression that determines if plugin logic should run for a given execution context.</summary>
        Expression<Func<IPluginExecutionContext, bool>> ValidationExpression { get; }

        /// <summary>Header label used in trace and error messages.</summary>
        string Header { get; }

        /// <summary>Core business logic implementation.</summary>
        void ExecutePlugin(IPluginContext context);

        /// <summary>Evaluates the validation expression against current context.</summary>
        bool IsContextValid();
    }
}
