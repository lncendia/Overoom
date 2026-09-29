using Common.Domain.Specifications.Abstractions;
using Films.Domain.Films.Specifications.Visitor;

namespace Films.Domain.Films.Specifications;

/// <summary>
/// Спецификация фильмов с указанными идентификаторами.
/// </summary>
/// <param name="ids">Идентификаторы фильмов</param>
public class FilmsByIdsSpecification(IReadOnlyCollection<Guid> ids) : ISpecification<Film, IFilmSpecificationVisitor>
{
  /// <summary>
  /// Идентификаторы фильмов
  /// </summary>
  public IReadOnlyCollection<Guid> Ids { get; } = ids;

  /// <inheritdoc/>
  public bool IsSatisfiedBy(Film item)
  {
    return Ids.Contains(item.Id);
  }

  /// <inheritdoc/>
  public void Accept(IFilmSpecificationVisitor visitor)
  {
    visitor.Visit(this);
  }
}
