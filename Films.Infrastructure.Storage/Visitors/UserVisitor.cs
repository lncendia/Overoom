using System.Linq.Expressions;
using Common.Domain.Specifications.Abstractions;
using Common.Infrastructure.Repositories.Visitors;
using Films.Domain.Users;
using Films.Domain.Users.Specifications.Visitor;
using Films.Infrastructure.Storage.Models.Users;

namespace Films.Infrastructure.Storage.Visitors;

public class UserVisitor : BaseSpecificationVisitor<UserModel, IUserSpecificationVisitor, User>, IUserSpecificationVisitor
{
    protected override Expression<Func<UserModel, bool>> ConvertSpecToExpression(
        ISpecification<User, IUserSpecificationVisitor> spec)
    {
        var visitor = new UserVisitor();
        spec.Accept(visitor);
        return visitor.Expr!;
    }
}