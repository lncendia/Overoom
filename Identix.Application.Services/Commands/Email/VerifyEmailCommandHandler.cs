using Common.IntegrationEvents.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
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
        // Поиск пользователя по идентификатору; если не найден, вызываем исключение UserNotFoundException.
        var user = await userManager.FindByIdAsync(request.UserId.ToString()) ?? throw new UserNotFoundException();

        // Начинаем транзакцию в контексте базы данных MongoDB.
        await dbContext.BeginTransaction(cancellationToken);
        
        // Попытка подтверждения электронной почты.
        var result = await userManager.ConfirmEmailAsync(user, request.Code);

        // Проверка успешности подтверждения; если не удалось, вызываем исключение InvalidCodeException.
        if (!result.Succeeded)
        {
            // Отмена транзакции
            await dbContext.AbortTransaction(cancellationToken);
            throw new InvalidCodeException();
        }

        // Публикуем событие
        await publishEndpoint.Publish(new UserRegisteredIntegrationEvent
        {
            Id = user.Id,
            PhotoKey = user.PhotoKey,
            Name = user.UserName!,
            Email = user.Email!,
            RegistrationTimeUtc = user.RegistrationTimeUtc,
            Locale = user.Locale.ToString()
        }, cancellationToken);
        
        // Фиксируем транзакцию в контексте базы данных MongoDB.
        await dbContext.CommitTransaction(cancellationToken);
    }
}