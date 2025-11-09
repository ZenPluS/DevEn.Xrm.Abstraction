using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;
using System;
using DevEn.Xrm.Abstraction.Plugins.Helpers;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class PluginExceptionHelperTests
    {
        [TestMethod]
        public void Categorize_Should_Return_BusinessRule_For_InvalidPluginExecution()
        {
            var cat = PluginExceptionHelper.Categorize(new InvalidPluginExecutionException("biz"));
            Assert.AreEqual(PluginErrorCategory.BusinessRule, cat);
        }

        [TestMethod]
        public void Categorize_Should_Return_Validation_For_ArgumentException()
        {
            var cat = PluginExceptionHelper.Categorize(new ArgumentException("bad arg"));
            Assert.AreEqual(PluginErrorCategory.Validation, cat);
        }

        [TestMethod]
        public void Categorize_Should_Return_Transient_For_Timeout_Message()
        {
            var cat = PluginExceptionHelper.Categorize(new Exception("operation timeout exceeded"));
            Assert.AreEqual(PluginErrorCategory.Transient, cat);
        }

        [TestMethod]
        public void Categorize_Should_Return_Security_For_Permission_Message()
        {
            var cat = PluginExceptionHelper.Categorize(new Exception("permission denied to update"));
            Assert.AreEqual(PluginErrorCategory.Security, cat);
        }

        [TestMethod]
        public void Categorize_Should_Return_Unknown_For_Other()
        {
            var cat = PluginExceptionHelper.Categorize(new Exception("random"));
            Assert.AreEqual(PluginErrorCategory.Unknown, cat);
        }
    }
}