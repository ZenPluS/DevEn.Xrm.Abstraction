using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq.Expressions;

namespace DevEn.Xrm.Abstraction.Plugins
{
    public sealed class ContactUpdatePlugin
        : BasePlugin
    {
        public override Expression<Func<IPluginExecutionContext, bool>> ValidationExpression
            => ctx => ctx.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase)
                  && ctx.PrimaryEntityName.Equals("contact", StringComparison.OrdinalIgnoreCase);

        public override void ExecutePlugin(IPluginContext context)
        {
            var tracing = context.TracingService;
            var service = context.SystemOrganizationService;
            var target = context.GetTargetEntity();

            if (target == null)
            {
                tracing.Trace("No target found.");
                return;
            }

            if (target.Contains("emailaddress1"))
            {
                var email = target["emailaddress1"]?.ToString();
                tracing.Trace("Updating description for email: " + email);

                var contact = service.Retrieve(target.LogicalName, target.Id, new ColumnSet("description"));
                contact["description"] = $"Updated by plugin at {DateTime.UtcNow:O}";
                service.Update(contact);
            }
        }
    }
}
