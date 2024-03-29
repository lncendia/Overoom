using Overoom.Domain.Films.Entities;
using Overoom.Domain.Films.Specifications.Visitor;
using Overoom.Domain.Specifications.Abstractions;

namespace Overoom.Domain.Films.Specifications;

public class FilmByScreenWriterSpecification : ISpecification<Film, IFilmSpecificationVisitor>
{
    public FilmByScreenWriterSpecification(string screenWriter) => ScreenWriter = screenWriter;

    public string ScreenWriter { get; }
    public bool IsSatisfiedBy(Film item) => item.FilmTags.Screenwriters.Any(x => x.ToUpper().Contains(ScreenWriter.ToUpper()));

    public void Accept(IFilmSpecificationVisitor visitor) => visitor.Visit(this);
}