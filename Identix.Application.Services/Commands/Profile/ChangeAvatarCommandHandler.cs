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
        // Поиск пользователя по идентификатору
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        // Вызываем исключение UserNotFoundException если не найден пользователь
        if (user == null) throw new UserNotFoundException();

        // Получаем предыдущую аватарку
        var oldThumbnail = user.PhotoKey;

        // Если до этого был установлен аватар
        if (oldThumbnail != null)
        {
            // Удаляем старый аватар
            await fileStore.DeleteAsync(oldThumbnail, token: cancellationToken);
        }

        // Формируем новый ключ для фото пользователя
        var newThumbnail = string.Format(Constants.Storage.UserPhotoKeyFormat, user.Id);

        // Сохраняем новый аватар локально
        await fileStore.UploadAsync(newThumbnail, request.Thumbnail, Constants.Storage.JpegMimeType,
            token: cancellationToken);

        // Создаем новый клайм
        var newClaim = new Claim(OpenIddictConstants.Claims.Picture, newThumbnail);

        // Начинаем транзакцию в контексте базы данных MongoDB.
        await dbContext.BeginTransaction(cancellationToken);

        // Если до этого был установлен аватар
        if (oldThumbnail != null)
        {
            // Сохраняем старый клайм
            var oldClaim = user.Claims.FirstOrDefault(c => c.ClaimType == OpenIddictConstants.Claims.Picture)!
                .ToClaim();

            // Заменяем клайм аватара
            await userManager.ReplaceClaimAsync(user, oldClaim, newClaim);
        }
        else
        {
            // Добавляем утверждение об аватаре
            await userManager.AddClaimAsync(user, newClaim);
        }

        // Публикуем событие
        await publishEndpoint.Publish(new UserInfoChangedIntegrationEvent
        {
            Id = user.Id,
            PhotoKey = user.PhotoKey,
            Name = user.UserName!,
            Email = user.Email!,
            Locale = user.Locale.ToString()
        }, cancellationToken);

        // Фиксируем транзакцию в контексте базы данных MongoDB.
        await dbContext.CommitTransaction(cancellationToken);

        // Возвращаем пользователя 
        return user;
    }
}