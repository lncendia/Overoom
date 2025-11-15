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
        // Пытаемся получить пользователя по логину
        var loginUser =
            await userManager.FindByLoginAsync(request.LoginInfo.LoginProvider, request.LoginInfo.ProviderKey);

        // Если пользователь существует вызываем исключение
        if (loginUser != null) throw new LoginAlreadyAssociatedException();

        // Пытаемся получить почту из утверждений, если почты нет - вызываем исключение
        var email = request.LoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? throw new EmailFormatException();

        // Пытаемся получить имя пользователя из утверждений, если нет - сплитим почту
        var username = request.LoginInfo.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];

        // Создаем пользователя
        var user = new AppUser
        {
            // Задаем электронную почту пользователя
            Email = email,

            // Задаем имя пользователя
            UserName = username.CutTo(40),

            // Задаем время регистрации пользователя в формате UTC
            RegistrationTimeUtc = DateTime.UtcNow,

            // Задаем время последней аутентификации пользователя в формате UTC
            LastAuthTimeUtc = DateTime.UtcNow,

            // Устанавливаем подтверждение электронной почты пользователя в значение true
            EmailConfirmed = true
        };

        // Начинаем транзакцию в контексте базы данных MongoDB.
        await dbContext.BeginTransaction(cancellationToken);

        // Сохраняем пользователя
        var result = await userManager.CreateAsync(user);

        // Если результат неудачный
        if (!result.Succeeded)
        {
            // Отмена транзакции
            await dbContext.AbortTransaction(cancellationToken);

            // Если хоть одна ошибка DuplicateEmail, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "DuplicateEmail")) throw new EmailAlreadyTakenException();

            // Если хоть одна ошибка InvalidEmail, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "InvalidEmail")) throw new EmailFormatException();

            // Если хоть одна ошибка InvalidUserNameLength, то вызываем исключение 
            if (result.Errors.Any(error => error.Code == "InvalidUserNameLength")) throw new UserNameLengthException();
        }

        // Создаем коллекцию утверждений пользователя и добавляем в нее локаль
        List<Claim> claims = [new(OpenIddictConstants.Claims.Locale, user.Locale.GetLocalizationString())];

        // Пытаемся получить ссылку на аватар из утверждений
        var thumbnailClaim = request.LoginInfo.Principal.FindFirstValue(Constants.Claims.Thumbnail);

        // Если аватар есть в утверждениях, то сохраняем его локально
        if (thumbnailClaim != null)
        {
            // Формируем новый ключ для фото пользователя
            var newThumbnail = string.Format(Constants.Storage.UserPhotoKeyFormat, user.Id);

            try
            {
                // Сохраняем фото
                await fileStore.UploadAsync(newThumbnail, new Uri(thumbnailClaim), Constants.Storage.JpegMimeType,
                    token: cancellationToken);

                // Так же добавляем фото профиля в утверждения пользователя
                claims.Add(new Claim(OpenIddictConstants.Claims.Picture, newThumbnail));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Не удалось сохранить фото пользователя {UserId} с адреса {PhotoUrl} по пути {FilePath} при регистрации через внешний провайдер",
                    user.Id, thumbnailClaim, newThumbnail);
            }
        }

        // Добавляем пользователю утверждения
        await userManager.AddClaimsAsync(user, claims);

        // Связываем пользователя с внешним провайдером
        await userManager.AddLoginAsync(user, request.LoginInfo);

        // Публикуем событие
        await publishEndpoint.Publish(new UserRegisteredIntegrationEvent
        {
            Id = user.Id,
            PhotoKey = user.PhotoKey,
            Name = user.UserName,
            Email = user.Email!,
            RegistrationTimeUtc = user.RegistrationTimeUtc,
            Locale = user.Locale.ToString()
        }, cancellationToken);

        // Фиксируем транзакцию в контексте базы данных MongoDB.
        await dbContext.CommitTransaction(cancellationToken);

        // Возвращаем пользователя
        return user;
    }
}