using AuthService.Application.Abstractions;
using AuthService.Application.Abstractions.Commands.Create;
using AuthService.Application.Abstractions.Entities;
using AuthService.Application.Abstractions.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PJMS.AuthService.Abstractions.Abstractions.AppEmailService;
using PJMS.AuthService.Abstractions.Abstractions.AppEmailService.Structs;

namespace AuthService.Application.Services.Commands.Create;

/// <summary>
/// Обработчик для выполнения команды создания пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="emailService">Сервис электронной почты для отправки уведомлений.</param>
public class CreateUserCommandHandler(UserManager<UserData> userManager, IEmailService emailService)
    : IRequestHandler<CreateUserCommand, UserData>
{
    /// <summary>
    /// Метод обработки команды создания пользователя.
    /// </summary>
    /// <param name="request">Запрос на создание пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Возвращает созданного пользователя в случае успеха.</returns>
    /// <exception cref="EmailFormatException">Вызывается, если пользователь уже существует.</exception>
    /// <exception cref="PasswordValidationException">Вызывается, если почта имеет неверный формат.</exception>
    /// <exception cref="EmailAlreadyTakenException">Вызывается, если валидация пароля не прошла.</exception>
    public async Task<UserData> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Создание нового объекта пользователя на основе данных из запроса.
        var user = new UserData(request.Email.Split('@')[0], request.Email, ApplicationConstants.DefaultAvatar,
            DateTime.Now);

        // Попытка создания пользователя с использованием UserManager.
        var result = await userManager.CreateAsync(user, request.Password);

        // Проверка успешности создания пользователя
        if (!result.Succeeded)
        {
            // Если хоть одна ошибка DuplicateEmail, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "DuplicateEmail")) throw new EmailAlreadyTakenException();

            // Если хоть одна ошибка InvalidEmail, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "InvalidEmail")) throw new EmailFormatException();
            
            // Если хоть одна ошибка InvalidUserName, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "InvalidUserName")) throw new UserNameFormatException();

            // Если хоть одна ошибка InvalidUserNameLength, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "InvalidUserNameLength")) throw new UserNameLengthException();

            // Создаем словарь для хранения ошибок
            var passwordValidationErrors = result.Errors.ToDictionary(e => e.Code, e => e.Description);

            // Вызываем исключение, содержащие в себе словарь ошибок валидации пароля
            throw new PasswordValidationException { ValidationErrors = passwordValidationErrors };
        }

        // Генерация кода подтверждения и формирование URL для подтверждения электронной почты.
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var url = GenerateMailUrl(request.ConfirmUrl, user.Id, code);

        // Отправка электронного письма с ссылкой для подтверждения регистрации.
        await emailService.SendAsync(new ConfirmRegistrationEmail { Recipient = request.Email, ConfirmLink = url });

        // Возвращение созданного пользователя.
        return user;
    }

    /// <summary>
    /// Генерирует URL для подтверждения регистрации по электронной почте.
    /// </summary>
    /// <param name="url">Базовый URL.</param>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="code">Код подтверждения.</param>
    /// <returns>Сгенерированный URL.</returns>
    private static string GenerateMailUrl(string url, Guid id, string code)
    {
        // Создаем объект UriBuilder с базовым URL
        var uriBuilder = new UriBuilder(url);

        // Получаем коллекцию параметров запроса
        var queryParameters = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        // Добавляем параметр "id" со значением
        queryParameters["userId"] = id.ToString();

        // Добавляем параметр "code" со значением
        queryParameters["code"] = code;

        // Устанавливаем обновленную строку запроса
        uriBuilder.Query = queryParameters.ToString();

        // Получаем обновленный URL
        return uriBuilder.ToString();
    }
}