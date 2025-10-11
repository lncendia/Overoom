using Common.Domain.Specifications.Abstractions;
using Films.Domain.Films.Specifications.Visitor;

namespace Films.Domain.Films.Specifications;

public class FilmsByDateSpecification(DateOnly date) : ISpecification<Film, IFilmSpecificationVisitor>
{
    public DateOnly Date { get; } = date;

    public void Accept(IFilmSpecificationVisitor visitor) => visitor.Visit(this);
    public bool IsSatisfiedBy(Film item) => item.Date == Date;
}