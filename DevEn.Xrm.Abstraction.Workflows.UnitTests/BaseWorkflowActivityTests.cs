using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Linq;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Workflows.UnitTests.Stubs;
using Microsoft.Xrm.Sdk.Workflow;
using DevEn.Xrm.Abstraction.Workflows.Abstract;
using DevEn.Xrm.Abstraction.Workflows.Core;

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

        private class ContactWorkflowValidator : ExpressionWorkflowContextValidator
        {
            public ContactWorkflowValidator() : base(ctx => ctx.PrimaryEntityName == "contact" && ctx.MessageName == "Update") { }
            public override string Description => "Update contact";
        }

        private class ValidatorActivity : BaseWorkflowActivity
        {
            public bool Ran;
            protected override IWorkflowContextValidator Validator => new ContactWorkflowValidator();
            protected override void ExecuteWorkflow(Core.IWorkflowActivityContext context, CodeActivityContext executionContext) => Ran = true;
        }

        private class ThrowingWorkflowValidator : IWorkflowContextValidator
        {
            public string Description => "Throwing WF";
            public Expression<Func<IWorkflowContext, bool>> Expression => _ => true; // not used
            public bool IsValid(IWorkflowContext context) => throw new InvalidOperationException("wf boom");
        }

        private class ThrowingValidatorActivity : BaseWorkflowActivity
        {
            public bool Ran;
            protected override IWorkflowContextValidator Validator => new ThrowingWorkflowValidator();
            protected override void ExecuteWorkflow(Core.IWorkflowActivityContext context, CodeActivityContext executionContext) => Ran = true;
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
            var act = new DummyActivity();
            BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService()).Invoke();
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
            var act = new InvalidActivity();
            BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService()).Invoke();
            Assert.IsFalse(act.Ran);
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Context not valid")), "Trace should indicate invalid context.");
        }

        [TestMethod]
        public void Validator_Activity_Should_Run_When_Validator_Passes()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "contact",
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var act = new ValidatorActivity();
            BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService()).Invoke();
            Assert.IsTrue(act.Ran);
        }

        [TestMethod]
        public void Validator_Activity_Should_Skip_When_Validator_Fails()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "account", // wrong entity
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var act = new ValidatorActivity();
            BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService()).Invoke();
            Assert.IsFalse(act.Ran);
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Context not valid")), "Trace should indicate invalid context.");
        }

        [TestMethod]
        public void Validator_Exception_Should_Be_Caught_And_Skip_Execution()
        {
            var tracing = new FakeTracingService();
            var wfCtx = new FakeWorkflowContext
            {
                PrimaryEntityName = "contact",
                MessageName = "Update",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            var act = new ThrowingValidatorActivity();
            BuildInvoker(act, wfCtx, tracing, new FakeOrganizationService()).Invoke();
            Assert.IsFalse(act.Ran, "Execution should be skipped on validator exception.");
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("ValidationExpression") || m.Contains("Throwing")), "Trace should show validation failure.");
        }
    }
}