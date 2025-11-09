using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using DevEn.Xrm.Abstraction.Plugins.Core;
using DevEn.Xrm.Abstraction.Plugins.CustomAttributes;
using DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class FilteredPluginBaseTests
    {
        [PluginFilter("Create", "contact")]
        [PluginFilter("Update", "contact")]
        private class ContactFilteredPlugin : FilteredPluginBase
        {
            public bool Ran;
            public override void ExecutePlugin(IPluginContext context) => Ran = true;
        }

        private static TestServiceProvider Build(FakeTracingService tracing, FakePluginExecutionContext ctx, IOrganizationService svc)
        {
            var factory = new FakeOrganizationServiceFactory(svc);
            var sp = new TestServiceProvider();
            sp.AddService<ITracingService>(tracing);
            sp.AddService<IPluginExecutionContext>(ctx);
            sp.AddService<IOrganizationServiceFactory>(factory);
            return sp;
        }

        [TestMethod]
        public void FilteredPlugin_Should_Run_For_Declared_Message_Entity()
        {
            var tracing = new FakeTracingService();
            var ctx = new FakePluginExecutionContext
            {
                MessageName = "Create",
                PrimaryEntityName = "contact",
                Depth = 1
            };
            var plugin = new ContactFilteredPlugin();
            plugin.Execute(Build(tracing, ctx, new FakeOrganizationService()));
            Assert.IsTrue(plugin.Ran);
        }

        [TestMethod]
        public void FilteredPlugin_Should_Skip_For_Other_Entity()
        {
            var tracing = new FakeTracingService();
            var ctx = new FakePluginExecutionContext
            {
                MessageName = "Create",
                PrimaryEntityName = "account",
                Depth = 1
            };
            var plugin = new ContactFilteredPlugin();
            plugin.Execute(Build(tracing, ctx, new FakeOrganizationService()));
            Assert.IsFalse(plugin.Ran);
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Context not valid")));
        }

        private class DepthLimitedPlugin : BasePlugin
        {
            protected override int? MaxAllowedDepth => 2;
            public bool Ran;
            public override System.Linq.Expressions.Expression<Func<IPluginExecutionContext, bool>> ValidationExpression => _ => true;
            public override void ExecutePlugin(IPluginContext context) => Ran = true;
        }

        [TestMethod]
        public void DepthLimitedPlugin_Should_Skip_When_Depth_Exceeds()
        {
            var tracing = new FakeTracingService();
            var ctx = new FakePluginExecutionContext
            {
                MessageName = "Update",
                PrimaryEntityName = "contact",
                Depth = 5
            };
            var plugin = new DepthLimitedPlugin();
            plugin.Execute(Build(tracing, ctx, new FakeOrganizationService()));
            Assert.IsFalse(plugin.Ran);
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Depth")));
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Summary") && m.Contains("SKIPPED")));
        }

        private class StageRequiredPlugin : BasePlugin
        {
            protected override int? RequiredStage => 20; // PreOperation
            public bool Ran;
            public override System.Linq.Expressions.Expression<Func<IPluginExecutionContext, bool>> ValidationExpression => _ => true;
            public override void ExecutePlugin(IPluginContext context) => Ran = true;
        }

        [TestMethod]
        public void StageRequiredPlugin_Should_Run_When_Stage_Matches()
        {
            var tracing = new FakeTracingService();
            var ctx = new FakePluginExecutionContext { MessageName = "Update", PrimaryEntityName = "contact", Stage = 20 };
            var plugin = new StageRequiredPlugin();
            plugin.Execute(Build(tracing, ctx, new FakeOrganizationService()));
            Assert.IsTrue(plugin.Ran);
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Summary") && m.Contains("SUCCESS")));
        }

        [TestMethod]
        public void StageRequiredPlugin_Should_Skip_When_Stage_Differs()
        {
            var tracing = new FakeTracingService();
            var ctx = new FakePluginExecutionContext { MessageName = "Update", PrimaryEntityName = "contact", Stage = 40 };
            var plugin = new StageRequiredPlugin();
            plugin.Execute(Build(tracing, ctx, new FakeOrganizationService()));
            Assert.IsFalse(plugin.Ran);
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("Stage")));
            Assert.IsTrue(tracing.Messages.Any(m => m.Contains("SKIPPED")));
        }
    }
}