using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Authentication;

/// <summary>
/// Обработчик команды для закрытия других сессий пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class UpdateSecurityStampCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<UpdateSecurityStampCommand, AppUser>
{
  /// <summary>
  /// Обработка команды на закрытие других сессий у пользователя.
  /// </summary>
  /// <param name="request">Запрос на закрытие сессий у пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает пользователя с обновленным Security Stamp в случае успеха.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  public async Task<AppUser> Handle(UpdateSecurityStampCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    await userManager.UpdateSecurityStampAsync(user);

    return user;
  }
}