using Common.Domain.Specifications.Abstractions;

namespace Films.Domain.Comments.Specifications.Visitor;

public interface ICommentSpecificationVisitor : ISpecificationVisitor<ICommentSpecificationVisitor, Comment>;