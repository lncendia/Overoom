using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Repositories;
using Films.Domain.Users;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Profile;

/// <summary>
/// Обработчик команды добавления/удаления фильма из списка просмотра пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям данных</param>
public class ToggleWatchListCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ToggleWatchListCommand>
{
  /// <summary>
  /// Переключает состояние фильма в списке "К просмотру" пользователя
  /// (добавляет, если отсутствует, или удаляет, если уже есть в списке)
  /// </summary>
  /// <param name="request">Команда с ID пользователя и ID фильма</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
  /// <exception cref="FilmNotFoundException">Если фильм не найден</exception>
  public async Task Handle(ToggleWatchListCommand request, CancellationToken cancellationToken)
  {
    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
    if (user == null) throw new UserNotFoundException(request.UserId);

    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);
    if (film == null) throw new FilmNotFoundException(request.FilmId);

    user.ToggleWatchlist(film);
    await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}