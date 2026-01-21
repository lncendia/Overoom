using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Films.Domain.Films;
using Films.Domain.Films.Specifications;
using Films.Domain.Films.Specifications.Visitor;
using Films.Infrastructure.Storage.Models.Films;

namespace Films.Infrastructure.Storage.Visitors;

public class FilmVisitor : BaseSpecificationVisitor<FilmModel, IFilmSpecificationVisitor, Film>,
  IFilmSpecificationVisitor
{
  protected override Expression<Func<FilmModel, bool>> ConvertSpecToExpression(
    ISpecification<Film, IFilmSpecificationVisitor> spec)
  {
    var visitor = new FilmVisitor();
    spec.Accept(visitor);
    return visitor.Expr!;
  }

  public void Visit(DuplicateFilmsSpecification specification)
  {
    Expr = model => model.Title == specification.Title
                    && model.Date.Year == specification.Date.Year
                    && model.Date.Month == specification.Date.Month
                    && model.Date.Day == specification.Date.Day;
  }
}