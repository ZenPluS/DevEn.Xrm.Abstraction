using System;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins.Helpers
{
    /// <summary>
    /// Categorizes exceptions into higher-level plugin error domains to aid diagnostics:
    /// BusinessRule (InvalidPluginExecutionException), Validation (ArgumentException),
    /// Transient (message contains 'timeout'), Security (message contains 'permission'), Unknown (fallback).
    /// </summary>
    public static class PluginExceptionHelper
    {
        public static Core.PluginErrorCategory Categorize(Exception ex)
        {
            switch (ex)
            {
                case InvalidPluginExecutionException _:
                    return Core.PluginErrorCategory.BusinessRule;
                case ArgumentException _:
                    return Core.PluginErrorCategory.Validation;
            }

            if (ex.Message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                return Core.PluginErrorCategory.Transient;

            return ex.Message.IndexOf("permission", StringComparison.OrdinalIgnoreCase) >= 0
                ? Core.PluginErrorCategory.Security
                : Core.PluginErrorCategory.Unknown;
        }
    }
}