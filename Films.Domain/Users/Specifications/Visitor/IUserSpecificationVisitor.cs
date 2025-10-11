using Common.Domain.Specifications.Abstractions;

namespace Films.Domain.Users.Specifications.Visitor;

public interface IUserSpecificationVisitor : ISpecificationVisitor<IUserSpecificationVisitor, User>;