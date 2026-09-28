using System.Security.Claims;
using Common.Application.FileStorage;
using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;

using MassTransit.MongoDbIntegration;

namespace Identix.Application.Services.Commands.Profile;

/// <summary>
/// Обработчик для смены аватара у пользователя
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="fileStore">Хранилище фотографий.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
/// <param name="dbContext">Контекст базы данных MongoDB</param>
public class ChangeAvatarCommandHandler(
  UserManager<AppUser> userManager,
  IFileStorage fileStore,
  IPublishEndpoint publishEndpoint,
  MongoDbContext dbContext)
  : IRequestHandler<ChangeAvatarCommand, AppUser>
{
  /// <summary>
  /// Метод обработки команды изменения аватара пользователя.
  /// </summary>
  /// <param name="request">Запрос на аватара у пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает обновленного пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  public async Task<AppUser> Handle(ChangeAvatarCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IList<Claim> claims = await userManager.GetClaimsAsync(user);
    string? oldThumbnail = AppUser.GetPhotoKey(claims);

    if (oldThumbnail != null)
    {
      await fileStore.DeleteAsync(oldThumbnail, token: cancellationToken);
    }

    string newThumbnail = string.Format(Constants.Storage.UserPhotoKeyFormat, user.Id);

    await fileStore.UploadAsync(newThumbnail, request.Thumbnail, Constants.Storage.JpegMimeType,
      token: cancellationToken);

    var newClaim = new Claim(OpenIddictConstants.Claims.Picture, newThumbnail);
    await dbContext.BeginTransaction(cancellationToken);

    if (oldThumbnail != null)
    {
      Claim oldClaim = claims.First(c => c.Type == OpenIddictConstants.Claims.Picture);
      await userManager.ReplaceClaimAsync(user, oldClaim, newClaim);
    }
    else
    {
      await userManager.AddClaimAsync(user, newClaim);
    }

    await publishEndpoint.Publish(new UserInfoChangedIntegrationEvent
    {
      Id = user.Id,
      PhotoKey = newClaim.Value,
      Name = user.UserName!,
      Email = user.Email!,
      Locale = AppUser.GetLocale(claims).GetLocalizationString()
    }, cancellationToken);

    await dbContext.CommitTransaction(cancellationToken);

    return user;
  }
}
