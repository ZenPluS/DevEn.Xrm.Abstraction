using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs;
using DevEn.Xrm.Abstraction.Plugins.Abstract;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class BasePluginTests
    {
        private class DummyPlugin : BasePlugin
        {
            public bool Executed;
            public override Expression<Func<IPluginExecutionContext, bool>> ValidationExpression => _ => true;
            public override void ExecutePlugin(IPluginContext context)
            {
                Executed = true;
            }
        }

        private class InvalidContextPlugin : BasePlugin
        {
            public bool Executed;
            public override Expression<Func<IPluginExecutionContext, bool>> ValidationExpression => _ => false;
            public override void ExecutePlugin(IPluginContext context)
            {
                Executed = true;
            }
        }

        private class CreateContactValidator : ExpressionPluginContextValidator
        {
            public CreateContactValidator() : base(ctx => ctx.MessageName == "Create" && ctx.PrimaryEntityName == "contact") { }
            public override string Description => "Create contact";
        }

        private class ValidatorPlugin : BasePlugin
        {
            public bool Executed;
            protected override IPluginContextValidator Validator => new CreateContactValidator();
            public override void ExecutePlugin(IPluginContext context) => Executed = true;
        }

        private class ThrowingValidator : IPluginContextValidator
        {
            public string Description => "Throwing";
            public Expression<Func<IPluginExecutionContext, bool>> Expression => _ => true; // not used
            public bool IsValid(IPluginExecutionContext context) => throw new InvalidOperationException("boom");
        }

        private class ThrowingValidatorPlugin : BasePlugin
        {
            public bool Executed;
            protected override IPluginContextValidator Validator => new ThrowingValidator();
            public override void ExecutePlugin(IPluginContext context) => Executed = true;
        }

        private static TestServiceProvider BuildServiceProvider(FakeTracingService tracing, FakePluginExecutionContext context, IOrganizationService service)
        {
            var factory = new FakeOrganizationServiceFactory(service);
            var sp = new TestServiceProvider();
            sp.AddService<ITracingService>(tracing);
            sp.AddService<IPluginExecutionContext>(context);
            sp.AddService<IOrganizationServiceFactory>(factory);
            return sp;
        }

        [TestMethod]
        public void Header_ShouldBe_ClassName()
        {
            var plugin = new DummyPlugin();
            Assert.AreEqual(nameof(DummyPlugin), plugin.Header);
        }

        [TestMethod]
        public void Execute_Should_Call_ExecutePlugin_When_Validation_Passes()
        {
            var tracing = new FakeTracingService();
            var context = new FakePluginExecutionContext
            {
                MessageName = "Create",
                PrimaryEntityName = "contact",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            var plugin = new DummyPlugin();
            plugin.Execute(BuildServiceProvider(tracing, context, new FakeOrganizationService()));

            Assert.IsTrue(plugin.Executed, "ExecutePlugin should have been invoked.");
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Start Execute")), "Start trace missing");
        }

        [TestMethod]
        public void Execute_Should_Skip_When_Validation_Fails()
        {
            var tracing = new FakeTracingService();
            var context = new FakePluginExecutionContext
            {
                MessageName = "Update",
                PrimaryEntityName = "account",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            var plugin = new InvalidContextPlugin();
            plugin.Execute(BuildServiceProvider(tracing, context, new FakeOrganizationService()));

            Assert.IsFalse(plugin.Executed, "ExecutePlugin should NOT have been invoked.");
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Context not valid")), "Trace should mention invalid context.");
        }

        [TestMethod]
        public void Validator_Plugin_Should_Run_When_Validator_Passes()
        {
            var tracing = new FakeTracingService();
            var context = new FakePluginExecutionContext
            {
                MessageName = "Create",
                PrimaryEntityName = "contact",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            var plugin = new ValidatorPlugin();
            plugin.Execute(BuildServiceProvider(tracing, context, new FakeOrganizationService()));

            Assert.IsTrue(plugin.Executed, "Validator-based plugin should execute when criteria met.");
        }

        [TestMethod]
        public void Validator_Plugin_Should_Skip_When_Validator_Fails()
        {
            var tracing = new FakeTracingService();
            var context = new FakePluginExecutionContext
            {
                MessageName = "Create",
                PrimaryEntityName = "account", // wrong entity
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            var plugin = new ValidatorPlugin();
            plugin.Execute(BuildServiceProvider(tracing, context, new FakeOrganizationService()));

            Assert.IsFalse(plugin.Executed, "Validator-based plugin should not execute when criteria fail.");
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Context not valid")), "Trace should indicate invalid context.");
        }

        [TestMethod]
        public void Validator_Exception_Should_Be_Caught_And_Skip_Execution()
        {
            var tracing = new FakeTracingService();
            var context = new FakePluginExecutionContext
            {
                MessageName = "Anything",
                PrimaryEntityName = "contact",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            var plugin = new ThrowingValidatorPlugin();
            plugin.Execute(BuildServiceProvider(tracing, context, new FakeOrganizationService()));

            Assert.IsFalse(plugin.Executed, "Execution should be skipped on validator exception.");
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Validation failure (Throwing)")), "Trace should include validation failure message.");
        }
    }
}