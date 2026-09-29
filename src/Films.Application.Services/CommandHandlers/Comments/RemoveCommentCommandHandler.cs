using Films.Application.Abstractions.Commands.Comments;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.CommentReactions.Specifications;
using Films.Domain.Comments;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Comments;

/// <summary>
/// Обработчик команды удаления комментария
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям</param>
public class RemoveCommentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveCommentCommand>
{
  /// <summary>
  /// Обрабатывает команду удаления комментария
  /// </summary>
  /// <param name="request">Команда с данными для удаления комментария</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="CommentNotFoundException">Выбрасывается, если комментарий не найден</exception>
  /// <exception cref="CommentNotBelongToUserException">Выбрасывается, если комментарий не принадлежит пользователю</exception>
  public async Task Handle(RemoveCommentCommand request, CancellationToken cancellationToken)
  {
    Comment? comment = await unitOfWork.CommentRepository.Value.GetAsync(request.CommentId, cancellationToken);
    if (comment == null) throw new CommentNotFoundException(request.CommentId);
    if (comment.UserId != request.UserId) throw new CommentNotBelongToUserException(request.UserId, request.CommentId);

    await unitOfWork.CommentRepository.Value.DeleteAsync(comment, cancellationToken);
    await unitOfWork.CommentReactionRepository.Value.DeleteRangeAsync(
      new CommentReactionsByCommentSpecification(comment.Id), cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}