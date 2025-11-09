using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Plugins.Diagnostics;

namespace DevEn.Xrm.Abstraction.Plugins
{
    /// <summary>
    /// Core plugin base implementing a consistent execution pipeline:
    /// 1. Construct <see cref="PluginContext"/> (lazy service access)
    /// 2. Trace start (message/entity/depth/stage)
    /// 3. Infrastructure validation (depth, stage)
    /// 4. Context validation (expression or validator abstraction)
    /// 5. Invoke derived <see cref="ExecutePlugin"/> on success
    /// 6. Emit completion summary with optional timing via <see cref="IExecutionDiagnostics"/>
    /// 7. Wrap any exception as <see cref="InvalidPluginExecutionException"/>
    /// Derive and supply either <see cref="ValidationExpression"/> override, or a <see cref="IPluginContextValidator"/>.
    /// </summary>
    public abstract class BasePlugin : IBasePlugin
    {
        private IPluginExecutionContext _context;
        private ITracingService _tracing;
        private IExecutionDiagnostics _diagnostics;

        /// <summary>Label used for tracing and error messages; defaults to type name.</summary>
        public virtual string Header => GetType().Name;

        /// <summary>Maximum allowed recursion depth; null disables depth check.</summary>
        protected virtual int? MaxAllowedDepth => null;

        /// <summary>Required pipeline stage; null accepts any.</summary>
        protected virtual int? RequiredStage => null;

        /// <summary>Optional validator abstraction providing richer metadata and predicate logic.</summary>
        protected virtual IPluginContextValidator Validator => null;

        /// <summary>Validation predicate (used only when <see cref="Validator"/> is null). True allows execution.</summary>
        public virtual Expression<Func<IPluginExecutionContext, bool>> ValidationExpression => Validator?.Expression;

        /// <summary>
        /// Main pipeline invoked by the Dataverse runtime.
        /// </summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            var pluginContext = new PluginContext(serviceProvider);
            _tracing = pluginContext.TracingService;
            _context = pluginContext.Context;
            _diagnostics = CreateDiagnostics();

            try
            {
                _tracing.Trace($"{Header} - Start Execute. Message: {_context.MessageName}, Entity: {_context.PrimaryEntityName}, Depth: {_context.Depth}, Stage: {_context.Stage}");

                if (!IsInfrastructureValid())
                {
                    _tracing.Trace($"{Header} - Infrastructure validation failed. Skipping.");
                    CompleteAndTraceSummary(skipped: true, error: null);
                    return;
                }

                if (!IsContextValid())
                {
                    _tracing.Trace($"Context not valid for plugin {Header}. Skipping.");
                    CompleteAndTraceSummary(skipped: true, error: null);
                    return;
                }

                ExecutePlugin(pluginContext);

                CompleteAndTraceSummary(skipped: false, error: null);
            }
            catch (Exception ex)
            {
                _tracing.Trace($"Exception: {Header} - {ex.Message}");
                CompleteAndTraceSummary(skipped: false, error: ex);
                throw new InvalidPluginExecutionException($"Error in {Header}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Factory method for diagnostics; override to disable or provide custom implementation.
        /// </summary>
        protected virtual IExecutionDiagnostics CreateDiagnostics() => new ExecutionDiagnostics();

        /// <summary>Derived plugin logic entry point.</summary>
        public abstract void ExecutePlugin(IPluginContext context);

        /// <summary>
        /// Checks recursion depth and stage constraints. Returns false when constraints violated.
        /// </summary>
        protected virtual bool IsInfrastructureValid()
        {
            if (MaxAllowedDepth.HasValue && _context.Depth > MaxAllowedDepth.Value)
            {
                _tracing.Trace($"{Header} - Depth {_context.Depth} exceeds MaxAllowedDepth {MaxAllowedDepth.Value}");
                return false;
            }

            if (!RequiredStage.HasValue || _context.Stage == RequiredStage.Value)
                return true;

            _tracing.Trace($"{Header} - Stage {_context.Stage} does not match RequiredStage {RequiredStage.Value}");
            return false;
        }

        /// <summary>
        /// Validates business context using validator abstraction or compiled expression.
        /// Exceptions during validation are traced and treated as failure.
        /// </summary>
        public virtual bool IsContextValid()
        {
            if (Validator != null)
                return SafeEval(() => Validator.IsValid(_context), Validator.Description);
            return ValidationExpression == null || SafeEval(() => ValidationExpression.Compile()(_context), "ValidationExpression");
        }

        private bool SafeEval(Func<bool> eval, string label)
        {
            try { return eval(); }
            catch (Exception ex)
            {
                _tracing?.Trace($"Validation failure ({label}): {ex.Message}");
                return false;
            }
        }

        private void CompleteAndTraceSummary(bool skipped, Exception error)
        {
            if (_diagnostics != null)
            {
                _diagnostics.MarkCompleted();
                _diagnostics.TraceSummary(_tracing, Header, skipped, error);
            }
            else
            {
                var status = error != null ? "FAILED" : (skipped ? "SKIPPED" : "SUCCESS");
                _tracing?.Trace($"{Header} - Summary Status={status}");
            }
        }
    }
}
