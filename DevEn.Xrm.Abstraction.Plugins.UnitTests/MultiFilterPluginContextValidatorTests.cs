using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevEn.Xrm.Abstraction.Plugins.Abstract;
using DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class MultiFilterPluginContextValidatorTests
    {
        [TestMethod]
        public void Validator_Should_Pass_For_Any_Filter()
        {
            var validator = new MultiFilterPluginContextValidator(new[]
            {
                ("Create","contact"),
                ("Update","account"),
                ("Delete","lead")
            });

            var ctx = new FakePluginExecutionContext { MessageName = "Update", PrimaryEntityName = "account" };
            Assert.IsTrue(validator.IsValid(ctx));
        }

        [TestMethod]
        public void Validator_Should_Fail_When_No_Filter_Matches()
        {
            var validator = new MultiFilterPluginContextValidator(new[]
            {
                ("Create","contact"),
                ("Update","account")
            });

            var ctx = new FakePluginExecutionContext { MessageName = "Delete", PrimaryEntityName = "lead" };
            Assert.IsFalse(validator.IsValid(ctx));
        }

        [TestMethod]
        public void Validator_Should_Allow_Entity_Null_Match_On_Message()
        {
            var validator = new MultiFilterPluginContextValidator(new[]
            {
                ("Create", (string)null)
            });

            var ctx = new FakePluginExecutionContext { MessageName = "Create", PrimaryEntityName = "anything" };
            Assert.IsTrue(validator.IsValid(ctx));
        }

        [TestMethod]
        public void Constructor_Should_Throw_On_Empty_List()
        {
            Assert.Throws<ArgumentException>(() => new MultiFilterPluginContextValidator(new (string,string)[] { }));
        }

        [TestMethod]
        public void Description_Should_Contain_Filters()
        {
            var validator = new MultiFilterPluginContextValidator(new[]
            {
                ("Create","contact"),
                ("Update","account")
            });
            Assert.Contains("Create/contact", validator.Description);
            Assert.Contains("Update/account", validator.Description);
        }
    }
}