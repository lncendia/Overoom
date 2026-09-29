using Films.Application.Abstractions.Commands.Comments;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.CommentReactions;
using Films.Domain.CommentReactions.Specifications;
using Films.Domain.Comments;
using Films.Domain.Repositories;
using Films.Domain.Users;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Comments;

/// <summary>
/// Обработчик команды установки или снятия реакции на комментарий.
/// Операция идемпотентна: повторная установка или снятие той же реакции ничего не меняет.
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям</param>
public class SetCommentReactionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SetCommentReactionCommand>
{
  /// <summary>
  /// Обрабатывает команду установки или снятия реакции
  /// </summary>
  /// <param name="request">Команда с данными о реакции</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="CommentNotFoundException">Если комментарий не найден</exception>
  /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
  public async Task Handle(SetCommentReactionCommand request, CancellationToken cancellationToken)
  {
    Comment? comment = await unitOfWork.CommentRepository.Value.GetAsync(request.CommentId, cancellationToken);
    if (comment == null || comment.FilmId != request.FilmId) throw new CommentNotFoundException(request.CommentId);

    var spec = new UserCommentReactionSpecification(request.CommentId, request.UserId, request.Reaction);
    CommentReaction? reaction = await unitOfWork.CommentReactionRepository.Value
      .FirstOrDefaultAsync(spec, cancellationToken);

    if (request.IsSet)
    {
      if (reaction != null) return;

      User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
      if (user == null) throw new UserNotFoundException(request.UserId);

      reaction = new CommentReaction(Guid.NewGuid(), comment, user, request.Reaction);
      await unitOfWork.CommentReactionRepository.Value.AddAsync(reaction, cancellationToken);
    }
    else
    {
      if (reaction == null) return;

      reaction.Remove();
      await unitOfWork.CommentReactionRepository.Value.DeleteAsync(reaction, cancellationToken);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
