using Common.Domain.Interfaces;
using Films.Domain.CommentReactions;
using Films.Domain.CommentReactions.Specifications.Visitor;

namespace Films.Domain.Repositories;

public interface ICommentReactionRepository
  : IRepository<CommentReaction, Guid, ICommentReactionSpecificationVisitor>;
