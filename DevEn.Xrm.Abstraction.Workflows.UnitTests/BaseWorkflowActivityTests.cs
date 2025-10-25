using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Linq;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Workflows;
using DevEn.Xrm.Abstraction.Workflows.UnitTests.Stubs;
using Microsoft.Xrm.Sdk.Workflow;

namespace DevEn.Xrm.Abstraction.Workflows.UnitTests
{
    [TestClass]
    public class BaseWorkflowActivityTests
    {
        private class DummyActivity : BaseWorkflowActivity
        {
            public bool Ran;
            protected override Expression<Func<IWorkflowContext, bool>> ValidationExpression => ctx => ctx.PrimaryEntityName == "contact";
            protected override void ExecuteWorkflow(Core.IWorkflowActivityContext context, CodeActivityContext executionContext)
            {
                Ran = true;
            }
        }

        private class InvalidActivity : BaseWorkflowActivity
        {
            public bool Ran;
            protected override Expression<Func<IWorkflowContext, bool>> ValidationExpression => ctx => ctx.PrimaryEntityName == "account";
            protected override void ExecuteWorkflow(Core.IWorkflowActivityContext context, CodeActivityContext executionContext)
            {
                Ran = true;
            }
        }

        private WorkflowInvoker BuildInvoker(object activity, FakeWorkflowContext wfCtx, FakeTracingService tracing, FakeOrganizationService service)
        {
            var invoker = new WorkflowInvoker(activity as System.Activities.Activity);
            invoker.Extensions.Add(() => tracing);
            invoker.Extensions.Add(() => wfCtx);
            invoker.Extensions.Add(() => new FakeOrganizationServiceFactory(service));
            return invoker;
        }

        [TestMethod]
        public void Activity_Should_Run_When_Validation_Passes()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "contact",
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var service = new FakeOrganizationService();
            var act = new DummyActivity();
            var invoker = BuildInvoker(act, wfCtx, tracing, service);

            invoker.Invoke();

            Assert.IsTrue(act.Ran);
        }

        [TestMethod]
        public void Activity_Should_Skip_When_Validation_Fails()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "contact",
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var service = new FakeOrganizationService();
            var act = new InvalidActivity();
            var invoker = BuildInvoker(act, wfCtx, tracing, service);

            invoker.Invoke();

            Assert.IsFalse(act.Ran);
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Context not valid")), "Trace should indicate invalid context.");
        }
    }
}