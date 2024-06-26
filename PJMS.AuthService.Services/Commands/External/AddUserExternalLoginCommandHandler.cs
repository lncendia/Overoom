using MediatR;
using Microsoft.AspNetCore.Identity;
using PJMS.AuthService.Abstractions.Commands.External;
using PJMS.AuthService.Abstractions.Entities;
using PJMS.AuthService.Abstractions.Exceptions;

namespace PJMS.AuthService.Services.Commands.External;

/// <summary>
/// Обработчик команды добавления внешнего логина для пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class AddUserExternalLoginCommandHandler(UserManager<AppUser> userManager)
    : IRequestHandler<AddUserExternalLoginCommand, AppUser>
{
    /// <summary>
    /// Метод обработки команды добавления внешнего логина для пользователя.
    /// </summary>
    /// <param name="request">Запрос на добавление внешнего логина.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Возвращает обновленного пользователя.</returns>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
    /// <exception cref="LoginAlreadyExistsException">Вызывается, если внешний логин уже существует.</exception>
    /// <exception cref="LoginAlreadyAssociatedException">Вызывается, если внешний логин уже ассоциирован с другим аккаунтом.</exception>
    public async Task<AppUser> Handle(AddUserExternalLoginCommand request, CancellationToken cancellationToken)
    {
        // Поиск пользователя по идентификатору; 
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        //Если не найден, вызываем исключение UserNotFoundException.
        if (user == null) throw new UserNotFoundException();

        // Получение всех внешних логинов пользователя.
        var logins = await userManager.GetLoginsAsync(user);

        // Проверка наличия внешнего логина с тем же провайдером; если есть, вызываем исключение LoginAlreadyExistsException.
        if (logins.Any(info => info.LoginProvider == request.LoginInfo.LoginProvider))
            throw new LoginAlreadyExistsException();

        // Попытка добавления внешнего логина.
        var result = await userManager.AddLoginAsync(user, request.LoginInfo);

        // Если операция не удалась, вызываем исключение LoginAlreadyAssociatedException.
        if (!result.Succeeded) throw new LoginAlreadyAssociatedException();

        // Возвращаем пользователя 
        return user;
    }
}