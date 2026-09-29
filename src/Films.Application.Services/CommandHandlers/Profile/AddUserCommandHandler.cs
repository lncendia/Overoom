using Films.Application.Abstractions.Commands.Profile;
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
    var user = new User(request.Id)
    {
      Username = request.UserName,

      PhotoKey = request.PhotoKey
    };

    await unitOfWork.UserRepository.Value.AddAsync(user, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
  }
}