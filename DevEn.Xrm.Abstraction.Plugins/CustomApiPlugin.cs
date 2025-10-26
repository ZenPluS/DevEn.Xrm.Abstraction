using System;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins
{
    /// <summary>
    /// Base class specialization for Dataverse Custom API plugins.
    /// Derive and provide the Custom API unique name via <see cref="CustomApiName"/> and optionally the bound entity logical name.
    /// Validation ensures message/entity matching before execution.
    /// </summary>
    public abstract class CustomApiPlugin
        : BasePlugin
    {
        /// <summary>
        /// Unique name (MessageName) of the Custom API this plugin handles (e.g. new_DoSomething).
        /// </summary>
        protected abstract string CustomApiName { get; }

        /// <summary>
        /// Logical name of the bound entity for the Custom API. Leave null or empty for unbound APIs.
        /// </summary>
        protected virtual string BoundEntityLogicalName => null;

        /// <summary>
        /// Validates that the current execution context message matches the Custom API name and,
        /// if specified, that the primary entity matches the bound entity logical name.
        /// </summary>
        public override Expression<Func<IPluginExecutionContext, bool>> ValidationExpression
            => ctx => ctx.MessageName.Equals(CustomApiName, StringComparison.OrdinalIgnoreCase)
                   && (string.IsNullOrEmpty(BoundEntityLogicalName)
                       || ctx.PrimaryEntityName.Equals(BoundEntityLogicalName, StringComparison.OrdinalIgnoreCase));
    }
}