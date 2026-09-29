using Common.Domain.Interfaces;
using Films.Domain.Users;
using Films.Domain.Users.Specifications.Visitor;

namespace Films.Domain.Repositories;

public interface IUserRepository : IRepository<User, Guid, IUserSpecificationVisitor>;