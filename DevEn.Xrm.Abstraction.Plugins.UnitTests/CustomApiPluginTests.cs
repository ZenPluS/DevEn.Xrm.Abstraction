using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Xrm.Sdk;
using DevEn.Xrm.Abstraction.Plugins;
using DevEn.Xrm.Abstraction.Plugins.Core;
using DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class CustomApiPluginTests
    {
        private TestServiceProvider BuildServiceProvider(FakePluginExecutionContext ctx, IOrganizationService orgService, FakeTracingService tracing)
        {
            var factory = new FakeOrganizationServiceFactory(orgService);
            var sp = new TestServiceProvider();
            sp.AddService<ITracingService>(tracing);
            sp.AddService<IPluginExecutionContext>(ctx);
            sp.AddService<IOrganizationServiceFactory>(factory);
            return sp;
        }

        private class UnboundTestCustomApiPlugin : CustomApiPlugin
        {
            public bool Executed;
            protected override string CustomApiName => "new_TestApi";
            public override void ExecutePlugin(IPluginContext context)
            {
                Executed = true;
                var input = context.GetInputParameter<string>("InputValue");
                context.SetOutputParameter("OutputValue", (input ?? string.Empty).ToUpperInvariant());
            }
        }

        private class BoundTestCustomApiPlugin : CustomApiPlugin
        {
            public bool Executed;
            protected override string CustomApiName => "new_BoundApi";
            protected override string BoundEntityLogicalName => "contact";
            public override void ExecutePlugin(IPluginContext context)
            {
                Executed = true;
                context.SetOutputParameter("BoundResult", "OK");
            }
        }

        [TestMethod]
        public void Unbound_CustomApi_Should_Set_Output_Parameter()
        {
            var tracing = new FakeTracingService();
            var orgService = new FakeOrganizationService();
            var ctx = new FakePluginExecutionContext
            {
                MessageName = "new_TestApi",
                PrimaryEntityName = string.Empty,
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            ctx.InputParameters.Add("InputValue", "hello");

            var sp = BuildServiceProvider(ctx, orgService, tracing);
            var plugin = new UnboundTestCustomApiPlugin();

            plugin.Execute(sp);

            Assert.IsTrue(plugin.Executed, "Plugin should execute for matching message.");
            Assert.IsTrue(ctx.OutputParameters.Contains("OutputValue"), "Output parameter should be set.");
            Assert.AreEqual("HELLO", ctx.OutputParameters["OutputValue"]);
        }

        [TestMethod]
        public void Unbound_CustomApi_Should_Not_Execute_On_Wrong_Message()
        {
            var tracing = new FakeTracingService();
            var orgService = new FakeOrganizationService();
            var ctx = new FakePluginExecutionContext
            {
                MessageName = "new_OtherApi",
                PrimaryEntityName = string.Empty,
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            ctx.InputParameters.Add("InputValue", "hello");

            var sp = BuildServiceProvider(ctx, orgService, tracing);
            var plugin = new UnboundTestCustomApiPlugin();

            plugin.Execute(sp);

            Assert.IsFalse(plugin.Executed, "Plugin should not execute for non-matching message.");
            Assert.IsFalse(ctx.OutputParameters.Contains("OutputValue"), "Output should not be set when not executed.");
        }

        [TestMethod]
        public void Bound_CustomApi_Should_Execute_For_Correct_Message_And_Entity()
        {
            var tracing = new FakeTracingService();
            var orgService = new FakeOrganizationService();
            var ctx = new FakePluginExecutionContext
            {
                MessageName = "new_BoundApi",
                PrimaryEntityName = "contact",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            var sp = BuildServiceProvider(ctx, orgService, tracing);
            var plugin = new BoundTestCustomApiPlugin();

            plugin.Execute(sp);

            Assert.IsTrue(plugin.Executed, "Bound plugin should execute for matching message/entity.");
            Assert.IsTrue(ctx.OutputParameters.Contains("BoundResult"));
            Assert.AreEqual("OK", ctx.OutputParameters["BoundResult"]);
        }

        [TestMethod]
        public void Bound_CustomApi_Should_Not_Execute_For_Wrong_Entity()
        {
            var tracing = new FakeTracingService();
            var orgService = new FakeOrganizationService();
            var ctx = new FakePluginExecutionContext
            {
                MessageName = "new_BoundApi",
                PrimaryEntityName = "account",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };

            var sp = BuildServiceProvider(ctx, orgService, tracing);
            var plugin = new BoundTestCustomApiPlugin();

            plugin.Execute(sp);

            Assert.IsFalse(plugin.Executed, "Bound plugin should not execute for wrong entity.");
            Assert.IsFalse(ctx.OutputParameters.Contains("BoundResult"), "Output should not be set.");
        }

        [TestMethod]
        public void CustomApiPlugin_Header_Should_Be_ClassName()
        {
            var plugin = new UnboundTestCustomApiPlugin();
            Assert.AreEqual(nameof(UnboundTestCustomApiPlugin), plugin.Header);
        }
    }
}