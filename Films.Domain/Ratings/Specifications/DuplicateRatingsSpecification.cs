using Common.Domain.Specifications.Abstractions;
using Films.Domain.Ratings.Specifications.Visitor;

namespace Films.Domain.Ratings.Specifications;

public class DuplicateRatingsSpecification(Guid filmId, Guid userId) : ISpecification<Rating, IRatingSpecificationVisitor>
{
  public Guid FilmId { get; } = filmId;
  
  public Guid UserId { get; } = userId;

  public void Accept(IRatingSpecificationVisitor visitor)
  {
    visitor.Visit(this);
  }

  public bool IsSatisfiedBy(Rating item)
  {
    return item.FilmId == FilmId && item.UserId == UserId;
  }
}