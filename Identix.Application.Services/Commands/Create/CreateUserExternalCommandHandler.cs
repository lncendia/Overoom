using System.Security.Claims;
using Common.Application.FileStorage;
using Common.IntegrationEvents.Users;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Create;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;
using MassTransit.MongoDbIntegration;

namespace Identix.Application.Services.Commands.Create;

/// <summary>
/// Обработчик команды создания пользователя с внешней учетной записью.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="fileStore">Хранилище фотографий.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
/// <param name="dbContext">Контекст базы данных MongoDB</param>
public class CreateUserExternalCommandHandler(
  UserManager<AppUser> userManager,
  IFileStorage fileStore,
  IPublishEndpoint publishEndpoint,
  MongoDbContext dbContext,
  ILogger<CreateUserExternalCommandHandler> logger)
  : IRequestHandler<CreateUserExternalCommand, AppUser>
{
  /// <summary>
  /// Метод обработки команды создания пользователя с внешней учетной записью.
  /// </summary>
  /// <param name="request">Запрос создания пользователя с внешней учетной записью.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает созданного пользователя в случае успеха.</returns>
  /// <exception cref="EmailFormatException">Вызывается, если почта имеет некорректный формат.</exception>
  /// <exception cref="EmailAlreadyTakenException">Вызывается, если почта уже используется другим пользователем.</exception>
  /// <exception cref="LoginAlreadyAssociatedException">Вызывается, если логин связан с другим пользователем.</exception>
  public async Task<AppUser> Handle(CreateUserExternalCommand request, CancellationToken cancellationToken)
  {
    AppUser? loginUser =
      await userManager.FindByLoginAsync(request.LoginInfo.LoginProvider, request.LoginInfo.ProviderKey);

    if (loginUser != null) throw new LoginAlreadyAssociatedException();

    string email = request.LoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? throw new EmailFormatException();
    string username = request.LoginInfo.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];

    var user = new AppUser
    {
      Email = email,
      UserName = username.CutTo(40),
      RegistrationTimeUtc = DateTime.UtcNow,
      LastAuthTimeUtc = DateTime.UtcNow,
      EmailConfirmed = true
    };

    await dbContext.BeginTransaction(cancellationToken);
    IdentityResult result = await userManager.CreateAsync(user);

    if (!result.Succeeded)
    {
      await dbContext.AbortTransaction(cancellationToken);
      if (result.Errors.Any(e => e.Code == "DuplicateEmail")) throw new EmailAlreadyTakenException();
      if (result.Errors.Any(e => e.Code == "InvalidEmail")) throw new EmailFormatException();
      if (result.Errors.Any(error => error.Code == "InvalidUserNameLength")) throw new UserNameLengthException();
    }

    List<Claim> claims = [new(OpenIddictConstants.Claims.Locale, request.Locale.GetLocalizationString())];
    string? thumbnailClaim = request.LoginInfo.Principal.FindFirstValue(Constants.Claims.Thumbnail);

    if (thumbnailClaim != null)
    {
      string newThumbnail = string.Format(Constants.Storage.UserPhotoKeyFormat, user.Id);

      try
      {
        await fileStore.UploadAsync(newThumbnail, new Uri(thumbnailClaim), Constants.Storage.JpegMimeType,
          token: cancellationToken);

        claims.Add(new Claim(OpenIddictConstants.Claims.Picture, newThumbnail));
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex,
          "Не удалось сохранить фото пользователя {UserId} с адреса {PhotoUrl} по пути {FilePath} при регистрации через внешний провайдер",
          user.Id, thumbnailClaim, newThumbnail);
      }
    }

    await userManager.AddClaimsAsync(user, claims);
    await userManager.AddLoginAsync(user, request.LoginInfo);

    await publishEndpoint.Publish(new UserRegisteredIntegrationEvent
    {
      Id = user.Id,
      PhotoKey = AppUser.GetPhotoKey(claims),
      Name = user.UserName,
      Email = user.Email!,
      RegistrationTimeUtc = user.RegistrationTimeUtc,
      Locale = request.Locale.GetLocalizationString()
    }, cancellationToken);

    await dbContext.CommitTransaction(cancellationToken);

    return user;
  }
}
