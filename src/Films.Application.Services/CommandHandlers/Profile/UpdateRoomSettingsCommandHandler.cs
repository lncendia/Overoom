using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Users;

using MediatR;

namespace Films.Application.Services.CommandHandlers.Profile;

/// <summary>
/// Обработчик команды изменения разрешений пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class UpdateRoomSettingsCommandHandler(IUnitOfWork unitOfWork)
  : IRequestHandler<UpdateRoomSettingsCommand>
{
  /// <summary>
  /// Обновляет настройки комнат для указанного пользователя
  /// </summary>
  /// <param name="request">Команда с новыми настройками разрешений</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
  public async Task Handle(UpdateRoomSettingsCommand request, CancellationToken cancellationToken)
  {
    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
    if (user == null) throw new UserNotFoundException(request.UserId);

    user.RoomSettings = request.Settings;
    await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }
}