using Common.Domain.Specifications.Abstractions;

namespace Films.Domain.Films.Specifications.Visitor;

public interface IFilmSpecificationVisitor : ISpecificationVisitor<IFilmSpecificationVisitor, Film>
{
    void Visit(FilmsByTitleSpecification specification);
    void Visit(FilmsByDateSpecification specification);
}