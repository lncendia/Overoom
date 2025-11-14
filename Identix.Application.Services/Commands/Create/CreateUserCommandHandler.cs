using System.Security.Claims;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Commands.Create;
using Identix.Application.Abstractions.Emails;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;
using MassTransit.MongoDbIntegration;

namespace Identix.Application.Services.Commands.Create;

/// <summary>
/// Обработчик для выполнения команды создания пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
/// <param name="dbContext">Контекст базы данных MongoDB</param>
public class CreateUserCommandHandler(
    UserManager<AppUser> userManager,
    IPublishEndpoint publishEndpoint,
    MongoDbContext dbContext)
    : IRequestHandler<CreateUserCommand, AppUser>
{
    /// <summary>
    /// Метод обработки команды создания пользователя.
    /// </summary>
    /// <param name="request">Запрос на создание пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Возвращает созданного пользователя в случае успеха.</returns>
    /// <exception cref="EmailAlreadyTakenException">Вызывается, если пользователь уже существует.</exception>
    /// <exception cref="EmailFormatException">Вызывается, если почта имеет неверный формат.</exception>
    /// <exception cref="PasswordValidationException">Вызывается, если валидация пароля не прошла.</exception>
    public async Task<AppUser> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Создание нового объекта пользователя на основе данных из запроса.
        var user = new AppUser
        {
            // Задает имя пользователя, извлекаемое из электронной почты путем разделения строки по символу '@' и выбора первой части.
            UserName = request.Email.Split('@')[0].CutTo(40),

            // Задает электронную почту пользователя.
            Email = request.Email,

            // Задает время регистрации пользователя в формате UTC.
            RegistrationTimeUtc = DateTime.UtcNow,

            // Задает время последней аутентификации пользователя в формате UTC.
            LastAuthTimeUtc = DateTime.UtcNow
        };

        // Начинаем транзакцию в контексте базы данных MongoDB.
        await dbContext.BeginTransaction(cancellationToken);

        // Попытка создания пользователя с использованием UserManager.
        var result = await userManager.CreateAsync(user, request.Password);

        // Проверка успешности создания пользователя
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

            // Создаем словарь для хранения ошибок
            var passwordValidationErrors = result.Errors.ToDictionary(e => e.Code, e => e.Description);

            // Вызываем исключение, содержащие в себе словарь ошибок валидации пароля
            throw new PasswordValidationException { ValidationErrors = passwordValidationErrors };
        }

        // Так же добавляем локализацию в утверждения пользователя
        await userManager.AddClaimAsync(user,
            new Claim(OpenIddictConstants.Claims.Locale, user.Locale.GetLocalizationString()));

        // Генерация кода подтверждения и формирование URL для подтверждения электронной почты.
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Генерация URL подтверждения почты
        var url = user.GenerateMailConfirmUrl(request.ConfirmUrl, code, request.ReturnUrl);

        // Отправка электронного письма со ссылкой для подтверждения регистрации.
        var message = new ConfirmRegistrationEmail { Recipient = request.Email, ConfirmLink = url };
        await publishEndpoint.Publish(new SendEmail { Message = message }, cancellationToken);

        // Фиксируем транзакцию в контексте базы данных MongoDB.
        await dbContext.CommitTransaction(cancellationToken);

        // Возвращение созданного пользователя.
        return user;
    }
}