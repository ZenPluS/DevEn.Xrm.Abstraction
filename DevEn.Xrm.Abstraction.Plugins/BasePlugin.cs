using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq.Expressions;

namespace DevEn.Xrm.Abstraction.Plugins
{
    public abstract class BasePlugin
        : IBasePlugin
    {
        private IPluginExecutionContext _context;
        private ITracingService _tracing;
        public abstract Expression<Func<IPluginExecutionContext, bool>> ValidationExpression { get; }
        public virtual string Header => GetType().Name;

        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginContext = new PluginContext(serviceProvider);
            _tracing = pluginContext.TracingService;
            _context = pluginContext.Context;

            try
            {
                _tracing.Trace($"{Header} - Start Execute. Message: {_context.MessageName}, Entity: {_context.PrimaryEntityName}");

                if (!IsContextValid())
                {
                    _tracing.Trace($"Context not valid for plugin {Header}. Skipping.");
                    return;
                }

                ExecutePlugin(pluginContext);
            }
            catch (Exception ex)
            {
                _tracing.Trace($"Exception: {Header} - {ex.Message}");
                throw new InvalidPluginExecutionException($"Error in {Header}: {ex.Message}", ex);
            }
        }

        public abstract void ExecutePlugin(IPluginContext context);

        public virtual bool IsContextValid()
        {
            if (ValidationExpression == null)
                return true;

            try
            {
                return ValidationExpression.Compile()(_context);
            }
            catch (Exception ex)
            {
                _tracing.Trace($"ValidationExpression exception: {ex.Message}");
                return false;
            }
        }
    }
}
