using Common.Application.Events;
using Common.Domain.Events;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using Films.Domain.Users;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания пользователя
/// Выполняет проверки перед сохранением нового пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class UserCreatedEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<CreateEvent<User>>
{
    /// <summary>
    /// Обрабатывает событие создания пользователя и выполняет валидацию
    /// </summary>
    /// <param name="notification">Доменное событие создания пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="UserAlreadyExistsException">Если пользователь с таким ID уже существует</exception>
    protected override async Task Execute(CreateEvent<User> notification, CancellationToken cancellationToken)
    {
        // Получаем пользователя по Id из события
        var user = await unitOfWork.UserRepository.Value.GetAsync(notification.Aggregate.Id, cancellationToken);

        // Проверяем, не существует ли уже пользователь с таким Id
        // Это предотвращает дублирование пользователей в системе
        if (user != null) throw new UserAlreadyExistsException(notification.Aggregate.Id);
    }
}