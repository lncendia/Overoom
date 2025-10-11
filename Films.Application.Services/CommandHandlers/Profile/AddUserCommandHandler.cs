using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Users;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Profile;

/// <summary>
/// Обработчик команды добавления нового пользователя в систему
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class AddUserCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddUserCommand>
{
    /// <summary>
    /// Обрабатывает запрос на создание нового пользователя
    /// </summary>
    /// <param name="request">Данные для создания пользователя (Id, UserName, PhotoUrl)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="UserAlreadyExistsException">Если пользователь с таким Id уже существует</exception>
    public async Task Handle(AddUserCommand request, CancellationToken cancellationToken)
    {
        // Создаем нового пользователя с указанными данными
        var user = new User(request.Id)
        {
            // Устанавливаем имя пользователя
            Username = request.UserName,
            
            // Устанавливаем ключ фото
            PhotoKey = request.PhotoKey
        };

        // Добавляем нового пользователя в репозиторий
        await unitOfWork.UserRepository.Value.AddAsync(user, cancellationToken);
        
        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}