using System;

namespace DevEn.Xrm.Abstraction.Plugins.CustomAttributes
{
    /// <summary>
    /// Attribute to define filtering criteria for plugins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PluginFilterAttribute : Attribute
    {
        public string Message { get; }
        public string EntityLogicalName { get; }

        public PluginFilterAttribute(string message, string entityLogicalName = null)
        {
            Message = message;
            EntityLogicalName = entityLogicalName;
        }
    }
}