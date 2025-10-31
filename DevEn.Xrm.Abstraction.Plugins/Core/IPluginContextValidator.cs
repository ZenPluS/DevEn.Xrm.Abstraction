using Microsoft.Xrm.Sdk;
using System;
using System.Linq.Expressions;

namespace DevEn.Xrm.Abstraction.Plugins.Core
{
    /// <summary>
    /// Defines a contract for validating plugin execution contexts using a logical expression and a validation method.
    /// </summary>
    /// <remarks>Implementations of this interface provide a description, a predicate expression for context
    /// validation, and a method to evaluate whether a given plugin execution context meets the required criteria. This
    /// interface is typically used to encapsulate validation logic for plugins in extensible systems.</remarks>
    public interface IPluginContextValidator
    {
        /// <summary>
        /// Gets the description associated with the current instance.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the predicate expression used to evaluate whether a given plugin execution context satisfies specific
        /// criteria.
        /// </summary>
        /// <remarks>The returned expression can be used to filter or match plugin execution contexts
        /// based on custom logic. This is typically utilized in scenarios where conditional processing of plugin events
        /// is required.</remarks>
        Expression<Func<IPluginExecutionContext, bool>> Expression { get; }

        /// <summary>
        /// Determines whether the specified plugin execution context meets the criteria for validity.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool IsValid(IPluginExecutionContext context);
    }
}