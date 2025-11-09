using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs;
using System;
using DevEn.Xrm.Abstraction.Plugins.Extensions;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class PluginContextExtensionsTests
    {
        private class TestPlugin : BasePlugin
        {
            public override System.Linq.Expressions.Expression<Func<IPluginExecutionContext, bool>> ValidationExpression => _ => true;
            public override void ExecutePlugin(Core.IPluginContext context) { }
        }

        private PluginContext BuildContext(Entity target, Entity preImage = null)
        {
            var tracing = new FakeTracingService();
            var execution = new FakePluginExecutionContext
            {
                MessageName = "Update",
                PrimaryEntityName = target?.LogicalName ?? "account",
                UserId = Guid.NewGuid(),
                InitiatingUserId = Guid.NewGuid()
            };
            if (target != null)
                execution.InputParameters["Target"] = target;
            if (preImage != null)
                execution.PreEntityImages["PreImage"] = preImage;
            var provider = new TestServiceProvider();
            provider.AddService<ITracingService>(tracing);
            provider.AddService<IPluginExecutionContext>(execution);
            provider.AddService<IOrganizationServiceFactory>(new FakeOrganizationServiceFactory(new FakeOrganizationService()));
            return new PluginContext(provider);
        }

        [TestMethod]
        public void AttributeChanged_Should_Return_True_When_Different()
        {
            var pre = new Entity("contact") { Id = Guid.NewGuid() };
            pre["telephone1"] = "111";
            var target = new Entity("contact") { Id = pre.Id };
            target["telephone1"] = "222";

            var ctx = BuildContext(target, pre);
            Assert.IsTrue(ctx.AttributeChanged("PreImage", "telephone1"));
        }

        [TestMethod]
        public void AttributeChanged_Should_Return_False_When_Same()
        {
            var pre = new Entity("contact") { Id = Guid.NewGuid() };
            pre["telephone1"] = "111";
            var target = new Entity("contact") { Id = pre.Id };
            target["telephone1"] = "111";

            var ctx = BuildContext(target, pre);
            Assert.IsFalse(ctx.AttributeChanged("PreImage", "telephone1"));
        }

        [TestMethod]
        public void GetString_Should_Get_From_Target_First()
        {
            var pre = new Entity("contact") { Id = Guid.NewGuid() };
            pre["description"] = "old";
            var target = new Entity("contact") { Id = pre.Id };
            target["description"] = "new";

            var ctx = BuildContext(target, pre);
            Assert.AreEqual("new", ctx.GetString("description", "PreImage"));
        }

        [TestMethod]
        public void GetString_Should_Fallback_To_PreImage()
        {
            var pre = new Entity("contact") { Id = Guid.NewGuid() };
            pre["description"] = "old";
            var target = new Entity("contact") { Id = pre.Id };

            var ctx = BuildContext(target, pre);
            Assert.AreEqual("old", ctx.GetString("description", "PreImage"));
        }

        [TestMethod]
        public void GetString_Should_Return_Null_When_Not_Found()
        {
            var ctx = BuildContext(new Entity("contact") { Id = Guid.NewGuid() });
            Assert.IsNull(ctx.GetString("nonexistent", "PreImage"));
        }
    }
}