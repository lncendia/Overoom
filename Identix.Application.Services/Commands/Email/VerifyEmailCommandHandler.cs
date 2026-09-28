using System.Security.Claims;

using Common.IntegrationEvents.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;

using MassTransit;
using MassTransit.MongoDbIntegration;

namespace Identix.Application.Services.Commands.Email;

/// <summary>
/// Обработчик команды подтверждения электронной почты пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
/// <param name="dbContext">Контекст базы данных MongoDB</param>
public class VerifyEmailCommandHandler(
  UserManager<AppUser> userManager,
  IPublishEndpoint publishEndpoint,
  MongoDbContext dbContext) : IRequestHandler<VerifyEmailCommand>
{
  /// <summary>
  /// Метод обработки команды подтверждения электронной почты пользователя.
  /// </summary>
  /// <param name="request">Запрос подтверждения электронной почты.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="InvalidCodeException">Вызывается, если код подтверждения недействителен.</exception>
  public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
  {
    AppUser user = await userManager.FindByIdAsync(request.UserId.ToString()) ?? throw new UserNotFoundException();
    await dbContext.BeginTransaction(cancellationToken);
    IdentityResult result = await userManager.ConfirmEmailAsync(user, request.Code);

    if (!result.Succeeded)
    {
      await dbContext.AbortTransaction(cancellationToken);
      throw new InvalidCodeException();
    }

    IList<Claim> claims = await userManager.GetClaimsAsync(user);

    await publishEndpoint.Publish(new UserRegisteredIntegrationEvent
    {
      Id = user.Id,
      PhotoKey = AppUser.GetPhotoKey(claims),
      Name = user.UserName!,
      Email = user.Email!,
      RegistrationTimeUtc = user.RegistrationTimeUtc,
      Locale = AppUser.GetLocale(claims).GetLocalizationString()
    }, cancellationToken);

    await dbContext.CommitTransaction(cancellationToken);
  }
}
