using Overoom.Domain.Films.Specifications.Visitor;
using Overoom.Domain.Specifications.Abstractions;

namespace Overoom.Domain.Films.Specifications;

public class FilmByScreenWriterSpecification : ISpecification<Film, IFilmSpecificationVisitor>
{
    public FilmByScreenWriterSpecification(string screenWriter) => ScreenWriter = screenWriter;

    public string ScreenWriter { get; }
    public bool IsSatisfiedBy(Film item) => item.FilmCollections.Screenwriters.Any(x => x == ScreenWriter);

    public void Accept(IFilmSpecificationVisitor visitor) => visitor.Visit(this);
}