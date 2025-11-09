namespace DevEn.Xrm.Abstraction.Plugins.Core
{
    /// <summary>High-level classification for plugin errors.</summary>
    public enum PluginErrorCategory
    {
        Unknown,
        Validation,
        BusinessRule,
        DependencyUnavailable,
        Transient,
        Security
    }
}