using System.Security.Claims;
using AuthService.Application.Abstractions.Commands.Create;
using AuthService.Application.Abstractions.Entities;
using AuthService.Application.Abstractions.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Services.Commands.Create;

/// <summary>
/// Обработчик команды создания пользователя с внешней учетной записью.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class CreateUserExternalCommandHandler(UserManager<UserData> userManager)
    : IRequestHandler<CreateUserExternalCommand, UserData>
{
    /// <summary>
    /// Метод обработки команды создания пользователя с внешней учетной записью.
    /// </summary>
    /// <param name="request">Запрос создания пользователя с внешней учетной записью.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Возвращает созданного пользователя в случае успеха.</returns>
    /// <exception cref="EmailAlreadyTakenException">Вызывается, если почта имеет некорректный формат.</exception>
    /// <exception cref="LoginAlreadyAssociatedException">Вызывается, если почта уже используется другим пользователем.</exception>
    /// <exception cref="EmailFormatException">Вызывается, если логин связан с другим пользователем.</exception>
    public async Task<UserData> Handle(CreateUserExternalCommand request, CancellationToken cancellationToken)
    {
        // Пытаемся получить пользователя по логину
        var loginUser =
            await userManager.FindByLoginAsync(request.LoginInfo.LoginProvider, request.LoginInfo.ProviderKey);

        // Если пользователь существует вызываем исключение
        if (loginUser != null) throw new LoginAlreadyAssociatedException();

        // Пытаемся получить почту из утверждений, если почты нет - вызываем исключение
        var email = request.LoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? throw new EmailFormatException();

        // Создаем пользователя
        var user = new UserData(email, request.Locale, DateTimeOffset.Now, DateTimeOffset.Now)
        {
            // Указываем, что почта подтверждена
            EmailConfirmed = true
        };

        // Сохраняем пользователя
        var result = await userManager.CreateAsync(user);

        // Если результат неудачный
        if (!result.Succeeded)
        {
            // Если хоть одна ошибка DuplicateEmail, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "DuplicateEmail")) throw new EmailAlreadyTakenException();

            // Если хоть одна ошибка InvalidEmail, то вызываем исключение 
            if (result.Errors.Any(e => e.Code == "InvalidEmail")) throw new EmailFormatException();
        }

        // Связываем пользователя с внешним провайдером
        await userManager.AddLoginAsync(user, request.LoginInfo);

        // Возвращаем пользователя
        return user;
    }
}