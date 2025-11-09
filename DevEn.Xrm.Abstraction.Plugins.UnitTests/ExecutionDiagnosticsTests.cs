using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevEn.Xrm.Abstraction.Plugins.Diagnostics;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class ExecutionDiagnosticsTests
    {
        [TestMethod]
        public void Diagnostics_Should_Set_Correlation_And_Start()
        {
            var diag = new ExecutionDiagnostics();
            Assert.AreNotEqual(Guid.Empty, diag.CorrelationId);
            Assert.IsLessThan(5, (DateTime.UtcNow - diag.StartedOn).TotalSeconds, "StartedOn too far in past.");
        }

        [TestMethod]
        public void Diagnostics_Should_Measure_Elapsed_After_Complete()
        {
            var diag = new ExecutionDiagnostics();
            System.Threading.Thread.Sleep(20);
            diag.MarkCompleted();
            var elapsed = diag.Elapsed;
            Assert.IsGreaterThanOrEqualTo(15, elapsed.TotalMilliseconds);
        }

        [TestMethod]
        public void TraceSummary_Should_Not_Throw()
        {
            var diag = new ExecutionDiagnostics();
            var tracing = new Stubs.FakeTracingService();
            diag.MarkCompleted();
            diag.TraceSummary(tracing, "TestHeader", skipped: false);
            Assert.IsNotEmpty(tracing.Messages);
            Assert.Contains("Summary", tracing.Messages[0]);
        }
    }
}