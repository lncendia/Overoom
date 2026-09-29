using System.Security.Claims;

using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;


namespace Identix.Application.Services.Commands.Profile;

/// <summary>
/// Обработчик для смены имени у пользователя
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class ChangeNameCommandHandler(
  UserManager<AppUser> userManager,
  IPublishEndpoint publishEndpoint) : IRequestHandler<ChangeNameCommand, AppUser>
{
  /// <summary>
  /// Метод обработки команды изменения имени пользователя.
  /// </summary>
  /// <param name="request">Запрос на смену имени у пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает обновленного пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="UserNameLengthException">Вызывается, если имя пользователя имеет некорректную длину.</exception>
  public async Task<AppUser> Handle(ChangeNameCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IList<Claim> claims = await userManager.GetClaimsAsync(user);
    IdentityResult result = await userManager.SetUserNameAsync(user, request.Name);

    if (!result.Succeeded)
    {
      if (result.Errors.Any(error => error.Code == "InvalidUserNameLength")) throw new UserNameLengthException();

      throw new InvalidOperationException(
        $"Failed to change user name: {string.Join(", ", result.Errors.Select(e => e.Code))}");
    }

    await publishEndpoint.Publish(new UserInfoChangedIntegrationEvent
    {
      Id = user.Id,
      PhotoKey = AppUser.GetPhotoKey(claims),
      Name = user.UserName!,
      Email = user.Email!,
      Locale = AppUser.GetLocale(claims).GetLocalizationString()
    }, cancellationToken);

    return user;
  }
}
