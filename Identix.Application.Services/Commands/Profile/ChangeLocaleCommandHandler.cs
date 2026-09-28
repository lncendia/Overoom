using System.Security.Claims;
using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;
using MassTransit.MongoDbIntegration;

namespace Identix.Application.Services.Commands.Profile;

/// <summary>
/// Обработчик для смены локали у пользователя
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
/// <param name="dbContext">Контекст базы данных MongoDB</param>
public class ChangeLocaleCommandHandler(
  UserManager<AppUser> userManager,
  IPublishEndpoint publishEndpoint,
  MongoDbContext dbContext)
  : IRequestHandler<ChangeLocaleCommand>
{
  /// <summary>
  /// Метод обработки команды изменения локали пользователя.
  /// </summary>
  /// <param name="request">Запрос на смену локали у пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает обновленного пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="UserNameLengthException">Вызывается, если имя пользователя имеет некорректную длину.</exception>
  public async Task Handle(ChangeLocaleCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IList<Claim> claims = await userManager.GetClaimsAsync(user);
    Claim? oldClaim = claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Locale);
    var newClaim = new Claim(OpenIddictConstants.Claims.Locale, request.Localization.GetLocalizationString());
    await dbContext.BeginTransaction(cancellationToken);

    if (oldClaim != null)
    {
      await userManager.ReplaceClaimAsync(user, oldClaim, newClaim);
    }
    else
    {
      await userManager.AddClaimAsync(user, newClaim);
    }

    await publishEndpoint.Publish(new UserInfoChangedIntegrationEvent
    {
      Id = user.Id,
      PhotoKey = AppUser.GetPhotoKey(claims),
      Name = user.UserName!,
      Email = user.Email!,
      Locale = newClaim.Value
    }, cancellationToken);

    await dbContext.CommitTransaction(cancellationToken);
  }
}
