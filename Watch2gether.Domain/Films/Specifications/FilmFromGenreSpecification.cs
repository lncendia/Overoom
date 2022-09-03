using Watch2gether.Domain.Films.Specifications.Visitor;
using Watch2gether.Domain.Specifications.Abstractions;

namespace Watch2gether.Domain.Films.Specifications;

public class FilmFromGenreSpecification : ISpecification<Film, IFilmSpecificationVisitor>
{
    public string Genre { get; }
    public FilmFromGenreSpecification(string genre) => Genre = genre;

    public bool IsSatisfiedBy(Film item) => item.FilmData.Genres.Select(x => x.ToLower()).Contains(Genre.ToLower());

    public void Accept(IFilmSpecificationVisitor visitor) => visitor.Visit(this);
}