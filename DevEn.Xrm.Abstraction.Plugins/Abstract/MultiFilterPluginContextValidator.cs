using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;
using DevEn.Xrm.Abstraction.Plugins.Core;

namespace DevEn.Xrm.Abstraction.Plugins.Abstract
{
    /// <summary>
    /// Validator that accepts a collection of (Message, Entity) tuples.
    /// </summary>
    public class MultiFilterPluginContextValidator : IPluginContextValidator
    {
        public Expression<Func<IPluginExecutionContext, bool>> Expression { get; }
        public string Description { get; }

        public MultiFilterPluginContextValidator(IEnumerable<(string Message, string Entity)> filters, string description = null)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));
            var list = filters.ToList();
            if (list.Count == 0) throw new ArgumentException("At least one filter required.", nameof(filters));

            Description = description ?? $"MultiFilter[{string.Join(";", list.Select(f => $"{f.Message}/{f.Entity}"))}]";

            // Build expression using a strongly-typed lambda instead of reflection-based property lookup to avoid
            // runtime failures on certain Microsoft.Xrm.Sdk interface implementations.
            Expression = ctx => list.Any(f =>
                string.Equals(ctx.MessageName, f.Message, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrEmpty(f.Entity) || string.Equals(ctx.PrimaryEntityName, f.Entity, StringComparison.OrdinalIgnoreCase)));
        }

        public bool IsValid(IPluginExecutionContext context) => Expression.Compile()(context);
    }
}