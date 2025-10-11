using Common.Infrastructure.Repositories;
using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Profile;

/// <summary>
/// Обработчик команды изменения разрешений пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class UpdateRoomSettingsCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork)
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
        // Получаем пользователя по ID из запроса
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);

        // Проверяем существование пользователя
        if (user == null) throw new UserNotFoundException(request.UserId);

        // Создаем новый объект разрешений с переданными параметрами
        user.RoomSettings = request.Settings;

        // Обновляем данные пользователя в репозитории
        await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);
        
        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateOutboxHandler(), cancellationToken);
    }
}