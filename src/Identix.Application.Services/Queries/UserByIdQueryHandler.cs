using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Identity;

using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса на получение пользователя по идентификатору.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class UserByIdQueryHandler(UserManager<AppUser> userManager) : IRequestHandler<UserByIdQuery, (AppUser user, ICollection<Claim> claims)>
{
  /// <summary>
  /// Метод обработки запроса на получение пользователя по идентификатору.
  /// </summary>
  /// <param name="request">Запрос на получение пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает пользователя по указанному идентификатору.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  public async Task<(AppUser user, ICollection<Claim> claims)> Handle(UserByIdQuery request, CancellationToken cancellationToken)
  {
    AppUser user = await userManager.FindByIdAsync(request.Id.ToString()) ?? throw new UserNotFoundException();

    IList<Claim> claims = await userManager.GetClaimsAsync(user);

    return (user, claims);
  }
}
