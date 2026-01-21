using Common.Domain.Specifications.Abstractions;
using Films.Domain.Films.Specifications.Visitor;

namespace Films.Domain.Films.Specifications;

public class DuplicateFilmsSpecification(string title, DateOnly date) : ISpecification<Film, IFilmSpecificationVisitor>
{
  public string Title { get; } = title;
  public DateOnly Date { get; } = date;
  public bool IsSatisfiedBy(Film item) => string.Equals(item.Title, Title) && item.Date == Date;

  public void Accept(IFilmSpecificationVisitor visitor) => visitor.Visit(this);
}