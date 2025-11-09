using System;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins.Diagnostics
{
    /// <summary>
    /// Default diagnostics implementation recording correlation id, start time, completion time and status.
    /// Produces a single summary trace (incl. elapsed milliseconds and optional error details).
    /// </summary>
    public class ExecutionDiagnostics
        : IExecutionDiagnostics
    {
        public Guid CorrelationId { get; } = Guid.NewGuid();
        public DateTime StartedOn { get; } = DateTime.UtcNow;
        public TimeSpan Elapsed => _end.HasValue ? _end.Value - StartedOn : DateTime.UtcNow - StartedOn;

        private DateTime? _end;

        public void MarkCompleted() => _end = DateTime.UtcNow;

        public void TraceSummary(ITracingService tracing, string header, bool skipped, Exception error = null)
        {
            if (tracing == null) return;
            var status = error != null ? "FAILED" : (skipped ? "SKIPPED" : "SUCCESS");
            tracing.Trace($"{header} - Summary CorrelationId={CorrelationId} Status={status} ElapsedMs={Elapsed.TotalMilliseconds:0.00}");
            if (error != null)
                tracing.Trace($"{header} - Error Detail: {error.GetType().Name} - {error.Message}");
        }
    }
}