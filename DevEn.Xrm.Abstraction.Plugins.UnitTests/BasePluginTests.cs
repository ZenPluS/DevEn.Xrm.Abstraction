using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Linq.Expressions;
using DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs;

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

            var orgService = new FakeOrganizationService();
            var factory = new FakeOrganizationServiceFactory(orgService);
            var sp = new TestServiceProvider();
            sp.AddService<ITracingService>(tracing);
            sp.AddService<IPluginExecutionContext>(context);
            sp.AddService<IOrganizationServiceFactory>(factory);

            var plugin = new DummyPlugin();
            plugin.Execute(sp);

            Assert.IsTrue(plugin.Executed, "ExecutePlugin should have been invoked.");
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

            var orgService = new FakeOrganizationService();
            var factory = new FakeOrganizationServiceFactory(orgService);
            var sp = new TestServiceProvider();
            sp.AddService<ITracingService>(tracing);
            sp.AddService<IPluginExecutionContext>(context);
            sp.AddService<IOrganizationServiceFactory>(factory);

            var plugin = new InvalidContextPlugin();
            plugin.Execute(sp);

            Assert.IsFalse(plugin.Executed, "ExecutePlugin should NOT have been invoked.");
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Context not valid")), "Trace should mention invalid context.");
        }
    }
}