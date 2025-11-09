    using System;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins.Core
{
    /// <summary>
    /// Provides structured diagnostics and timing for plugin executions.
    /// </summary>
    public interface IExecutionDiagnostics
    {
        Guid CorrelationId { get; }
        DateTime StartedOn { get; }
        TimeSpan Elapsed { get; }
        void MarkCompleted();
        void TraceSummary(ITracingService tracing, string header, bool skipped, Exception error = null);
    }
}