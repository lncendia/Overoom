using Common.Domain.Interfaces;
using Films.Domain.Ratings;
using Films.Domain.Ratings.Specifications.Visitor;

namespace Films.Domain.Repositories;

public interface IRatingRepository : IRepository<Rating, Guid, IRatingSpecificationVisitor>;