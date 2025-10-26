using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq.Expressions;

namespace DevEn.Xrm.Abstraction.Plugins
{
    /// <summary>
    /// Base class for all Dataverse plugins in this abstraction layer.
    /// Provides common execution flow: tracing, context validation, error handling.
    /// Derive and implement <see cref="ValidationExpression"/> and <see cref="ExecutePlugin(IPluginContext)"/>.
    /// </summary>
    public abstract class BasePlugin
        : IBasePlugin
    {
        private IPluginExecutionContext _context;
        private ITracingService _tracing;

        /// <summary>
        /// Expression used to validate whether the current <see cref="IPluginExecutionContext"/> matches plugin criteria
        /// (e.g. message name and entity). Return true to allow execution. Return false to skip.
        /// </summary>
        public abstract Expression<Func<IPluginExecutionContext, bool>> ValidationExpression { get; }

        /// <summary>
        /// Header label used in tracing and error messages. Defaults to the derived class name.
        /// </summary>
        public virtual string Header => GetType().Name;

        /// <summary>
        /// Entry point invoked by Dataverse runtime. Creates a <see cref="PluginContext"/> wrapper,
        /// traces start, validates context, executes business logic, and handles exceptions.
        /// </summary>
        /// <param name="serviceProvider">Service provider supplied by Dataverse pipeline.</param>
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

        /// <summary>
        /// Business logic implementation for the plugin. Use the provided <paramref name="context"/> to access services,
        /// input/output parameters, images and tracing.
        /// </summary>
        /// <param name="context">Abstraction of the execution context and services.</param>
        public abstract void ExecutePlugin(IPluginContext context);

        /// <summary>
        /// Evaluates the <see cref="ValidationExpression"/> against the current execution context.
        /// Returns true if execution should proceed, false otherwise. Traces any validation exceptions.
        /// </summary>
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
