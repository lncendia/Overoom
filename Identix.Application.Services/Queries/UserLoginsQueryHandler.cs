using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса на получение списка внешних учетных записей пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class UserLoginsQueryHandler(UserManager<AppUser> userManager)
  : IRequestHandler<UserLoginsQuery, IReadOnlyCollection<string>>
{
  /// <summary>
  /// Метод обработки запроса на получение списка внешних учетных записей пользователя.
  /// </summary>
  /// <param name="request">Запрос на получение списка внешних учетных записей пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает список внешних учетных записей пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  public async Task<IReadOnlyCollection<string>> Handle(UserLoginsQuery request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.Id.ToString());
    if (user == null) throw new UserNotFoundException();

    IList<UserLoginInfo> logins = await userManager.GetLoginsAsync(user);

    return [.. logins.Select(info => info.LoginProvider)];
  }
}