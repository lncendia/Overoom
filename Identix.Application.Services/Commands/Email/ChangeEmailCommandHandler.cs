using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Email;

/// <summary>
/// Обработчик для смены почта у пользователя
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class ChangeEmailCommandHandler(UserManager<AppUser> userManager) : IRequestHandler<ChangeEmailCommand, AppUser>
{
  /// <summary>
  /// Метод обработки команды изменения электронной почты пользователя.
  /// </summary>
  /// <param name="request">Запрос на смену почты у пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает обновленного пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  public async Task<AppUser> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IdentityResult result = await userManager.ChangeEmailAsync(user, request.NewEmail, request.Code);

    if (!result.Succeeded)
    {
      if (result.Errors.Any(error => error.Code == "DuplicateEmail")) throw new EmailAlreadyTakenException();
      if (result.Errors.Any(error => error.Code == "InvalidToken")) throw new InvalidCodeException();
      if (result.Errors.Any(e => e.Code == "InvalidEmail")) throw new EmailFormatException();
    }

    return user;
  }
}