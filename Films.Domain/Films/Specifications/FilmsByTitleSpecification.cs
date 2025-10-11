using Common.Domain.Specifications.Abstractions;
using Films.Domain.Films.Specifications.Visitor;

namespace Films.Domain.Films.Specifications;

public class FilmsByTitleSpecification(string title) : ISpecification<Film, IFilmSpecificationVisitor>
{
    public string Title { get; } = title;
    public bool IsSatisfiedBy(Film item) => item.Title.Contains(Title);

    public void Accept(IFilmSpecificationVisitor visitor) => visitor.Visit(this);
}