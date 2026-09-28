using Films.Application.Abstractions.Commands.Comments;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Comments;
using Films.Domain.Films;
using Films.Domain.Repositories;
using Films.Domain.Users;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Comments;

/// <summary>
/// Обработчик команды добавления комментария
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям</param>
public class AddCommentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddCommentCommand, Guid>
{
  /// <summary>
  /// Обрабатывает команду добавления нового комментария
  /// </summary>
  /// <param name="request">Команда с данными для создания комментария</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>DTO созданного комментария</returns>
  /// <exception cref="UserNotFoundException">Выбрасывается, если пользователь не найден</exception>
  /// <exception cref="FilmNotFoundException">Выбрасывается, если фильм не найден</exception>
  public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
  {
    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
    if (user == null) throw new UserNotFoundException(request.UserId);

    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);
    if (film == null) throw new FilmNotFoundException(request.FilmId);

    var comment = new Comment(Guid.NewGuid(), film, user, request.Text);
    await unitOfWork.CommentRepository.Value.AddAsync(comment, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

    return comment.Id;
  }
}
