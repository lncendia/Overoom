using Common.Domain.Interfaces;
using Films.Domain.Comments;
using Films.Domain.Comments.Specifications.Visitor;

namespace Films.Domain.Repositories;

public interface ICommentRepository : IRepository<Comment,Guid,ICommentSpecificationVisitor>;