using NpgsqlTypes;

namespace Data.Entities;

/// <summary>
/// Base class for entities that support PostgreSQL full-text search.
/// Derived entities configure which columns contribute to <see cref="SearchVector"/>
/// via <c>HasGeneratedTsVectorColumn</c> in their EF configuration, backed by a GIN index.
/// </summary>
public abstract class SearchableEntity
{
    public NpgsqlTsVector SearchVector { get; set; } = null!;
}
