using Common.Domain.Specifications.Abstractions;

namespace Films.Domain.Ratings.Specifications.Visitor;

public interface IRatingSpecificationVisitor : ISpecificationVisitor<IRatingSpecificationVisitor, Rating>
{
  void Visit(DuplicateRatingsSpecification specification);
}