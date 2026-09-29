using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.External;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.External;

/// <summary>
/// Обработчик команды удаления внешнего логина пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class RemoveUserExternalLoginCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<RemoveUserExternalLoginCommand, AppUser>
{
  /// <summary>
  /// Метод обработки команды удаления внешнего логина пользователя.
  /// </summary>
  /// <param name="request">Команда удаления внешнего логина.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает обновленного пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="LoginNotFoundException">Вызывается, если внешний логин не найден.</exception>
  public async Task<AppUser> Handle(RemoveUserExternalLoginCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IList<UserLoginInfo> logins = await userManager.GetLoginsAsync(user);
    UserLoginInfo? login = logins.FirstOrDefault(info => info.LoginProvider == request.Provider);
    if (login == null) throw new LoginNotFoundException();

    await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);

    return user;
  }
}