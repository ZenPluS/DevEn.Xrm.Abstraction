using DevEn.Xrm.Abstraction.Plugins.Core;

namespace DevEn.Xrm.Abstraction.Plugins.Extensions
{
    /// <summary>
    /// Convenience extension helpers for common attribute/image access patterns in plugin execution.
    /// </summary>
    public static class PluginContextExtension
    {
        /// <summary>
        /// Determines if a specific attribute changed between the pre-image and the target entity.
        /// Returns false if either image/target missing or attribute absent in target.
        /// </summary>
        public static bool AttributeChanged(this IPluginContext ctx, string preImageName, string attributeLogicalName)
        {
            var target = ctx.GetTargetEntity();
            var pre = ctx.GetPreImage(preImageName);
            if (target == null || pre == null) return false;
            if (!target.Attributes.Contains(attributeLogicalName)) return false;

            var newVal = target[attributeLogicalName];
            var oldVal = pre.Contains(attributeLogicalName) ? pre[attributeLogicalName] : null;
            return !Equals(newVal, oldVal);
        }

        /// <summary>
        /// Attempts to read a string attribute from target first, then (optionally) from specified pre-image.
        /// Returns null if not found in either source.
        /// </summary>
        public static string GetString(this IPluginContext ctx, string attributeLogicalName, string preImageName = null)
        {
            var target = ctx.GetTargetEntity();
            if (target?.Attributes.Contains(attributeLogicalName) == true)
                return target[attributeLogicalName] as string;

            if (string.IsNullOrEmpty(preImageName))
                return null;

            var pre = ctx.GetPreImage(preImageName);
            if (pre?.Attributes.Contains(attributeLogicalName) == true)
                return pre[attributeLogicalName] as string;
            return null;
        }
    }
}