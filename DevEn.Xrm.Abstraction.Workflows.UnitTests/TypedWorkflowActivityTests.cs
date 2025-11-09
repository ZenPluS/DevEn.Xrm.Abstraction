using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Collections.Generic;
using DevEn.Xrm.Abstraction.Workflows.Core;
using DevEn.Xrm.Abstraction.Workflows.UnitTests.Stubs;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows.UnitTests
{
    [TestClass]
    public class TypedWorkflowActivityTests
    {
        private class EchoActivity : TypedWorkflowActivity<string, string>
        {
            protected override System.Linq.Expressions.Expression<Func<IWorkflowContext, bool>> ValidationExpression
                => ctx => ctx.PrimaryEntityName == "contact";
            protected override string ExecuteCore(IWorkflowActivityContext ctx, string input)
                => $"Echo:{input}";
        }

        private class FailingActivity : TypedWorkflowActivity<string, string>
        {
            protected override System.Linq.Expressions.Expression<Func<IWorkflowContext, bool>> ValidationExpression
                => ctx => true;
            protected override string ExecuteCore(IWorkflowActivityContext ctx, string input)
                => throw new InvalidOperationException("boom");
        }

        private WorkflowInvoker BuildInvoker(Activity activity, FakeWorkflowContext wfCtx, FakeTracingService tracing, FakeOrganizationService service)
        {
            var invoker = new WorkflowInvoker(activity);
            invoker.Extensions.Add(() => tracing);
            invoker.Extensions.Add(() => wfCtx);
            invoker.Extensions.Add(() => new FakeOrganizationServiceFactory(service));
            return invoker;
        }

        [TestMethod]
        public void TypedActivity_Should_Set_Output_When_Valid()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "contact",
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var act = new EchoActivity();
            var results = BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService())
                .Invoke(new Dictionary<string, object> { { "Input", "Hello" } });

            Assert.IsTrue(results.ContainsKey("Result"));
            Assert.AreEqual("Echo:Hello", results["Result"]);
        }

        [TestMethod]
        public void TypedActivity_Should_Skip_When_Validation_Fails()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "account", // fails validation
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var act = new EchoActivity();
            var results = BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService())
                .Invoke(new Dictionary<string, object> { { "Input", "Hello" } });

            // OutArgument exists but should remain unset (null/default)
            Assert.IsTrue(results.ContainsKey("Result"));
            Assert.IsNull(results["Result"]);
        }

        [TestMethod]
        public void TypedActivity_Should_Wrap_Exception_As_InvalidWorkflowException()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "contact",
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var act = new FailingActivity();

            Assert.Throws<InvalidWorkflowException>(() =>
                BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService())
                    .Invoke(new Dictionary<string, object> { { "Input", "Hi" } }));
        }
    }
}