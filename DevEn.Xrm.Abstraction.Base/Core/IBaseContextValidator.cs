public interface IBaseContextValidator<T>
{
    /// <summary>
    /// Gets the description associated with this instance.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the LINQ expression used to filter entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>The returned expression can be used in LINQ queries to specify filtering criteria. This
    /// property is typically used to build dynamic queries or to encapsulate reusable filtering logic.</remarks>
    Expression<Func<T, bool>> Expression { get; }

    /// <summary>
    /// Determines whether the specified context satisfies the validation criteria.
    /// </summary>
    /// <param name="context">The context object to validate. The criteria for validity depend on the implementation.</param>
    /// <returns>true if the context is valid according to the implemented rules; otherwise, false.</returns>
    bool IsValid(T context);
}