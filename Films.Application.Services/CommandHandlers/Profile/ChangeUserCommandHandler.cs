using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
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
        // Получаем пользователя по ID из запроса
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.Id, cancellationToken);
        
        // Проверяем существование пользователя
        if (user == null) throw new UserNotFoundException(request.Id);

        // Обновляем имя пользователя
        user.Username = request.UserName;
        
        // Обновляем ключ аватара пользователя
        user.PhotoKey = request.PhotoKey;

        // Сохраняем обновленные данные в репозитории
        await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);
        
        // Фиксируем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}