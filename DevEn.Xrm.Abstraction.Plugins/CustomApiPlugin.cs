using System;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins
{
    public abstract class CustomApiPlugin
        : BasePlugin
    {
        protected abstract string CustomApiName { get; }
        protected virtual string BoundEntityLogicalName => null;

        public override Expression<Func<IPluginExecutionContext, bool>> ValidationExpression
            => ctx => ctx.MessageName.Equals(CustomApiName, StringComparison.OrdinalIgnoreCase)
                   && (string.IsNullOrEmpty(BoundEntityLogicalName)
                       || ctx.PrimaryEntityName.Equals(BoundEntityLogicalName, StringComparison.OrdinalIgnoreCase));
    }
}