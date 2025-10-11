using Common.Domain.Interfaces;
using Films.Domain.Films;
using Films.Domain.Films.Specifications.Visitor;

namespace Films.Domain.Repositories;

public interface IFilmRepository : IRepository<Film, Guid, IFilmSpecificationVisitor>;