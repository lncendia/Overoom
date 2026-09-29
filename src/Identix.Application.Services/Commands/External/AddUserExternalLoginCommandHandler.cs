using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.External;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.External;

/// <summary>
/// Обработчик команды добавления внешнего логина для пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class AddUserExternalLoginCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<AddUserExternalLoginCommand, AppUser>
{
  /// <summary>
  /// Метод обработки команды добавления внешнего логина для пользователя.
  /// </summary>
  /// <param name="request">Запрос на добавление внешнего логина.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает обновленного пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="LoginAlreadyExistsException">Вызывается, если внешний логин уже существует.</exception>
  /// <exception cref="LoginAlreadyAssociatedException">Вызывается, если внешний логин уже ассоциирован с другим аккаунтом.</exception>
  public async Task<AppUser> Handle(AddUserExternalLoginCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IList<UserLoginInfo> logins = await userManager.GetLoginsAsync(user);

    if (logins.Any(info => info.LoginProvider == request.LoginInfo.LoginProvider))
      throw new LoginAlreadyExistsException();

    IdentityResult result = await userManager.AddLoginAsync(user, request.LoginInfo);
    if (!result.Succeeded) throw new LoginAlreadyAssociatedException();

    return user;
  }
}