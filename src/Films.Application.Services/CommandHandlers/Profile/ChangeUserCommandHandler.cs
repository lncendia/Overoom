using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Users;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Profile;

/// <summary>
/// Обработчик команды изменения данных пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class ChangeUserCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeUserCommand>
{
  /// <summary>
  /// Обновляет основные данные пользователя
  /// </summary>
  /// <param name="request">Команда с новыми данными пользователя</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="UserNotFoundException">Если пользователь с указанным ID не найден</exception>
  public async Task Handle(ChangeUserCommand request, CancellationToken cancellationToken)
  {
    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.Id, cancellationToken);
    if (user == null) throw new UserNotFoundException(request.Id);

    user.Username = request.UserName;
    user.PhotoKey = request.PhotoKey;
    await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}