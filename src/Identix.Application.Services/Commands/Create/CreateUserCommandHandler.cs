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

namespace Identix.Application.Services.Commands.Create;

/// <summary>
/// Обработчик для выполнения команды создания пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class CreateUserCommandHandler(
  UserManager<AppUser> userManager,
  IPublishEndpoint publishEndpoint)
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
    var user = new AppUser
    {
      Id = Guid.NewGuid(),
      UserName = request.Email.Split('@')[0].CutTo(40),
      Email = request.Email,
      RegistrationTimeUtc = DateTime.UtcNow,
      LastAuthTimeUtc = DateTime.UtcNow
    };

    IdentityResult result = await userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      if (result.Errors.Any(e => e.Code == "DuplicateEmail")) throw new EmailAlreadyTakenException();
      if (result.Errors.Any(e => e.Code == "InvalidEmail")) throw new EmailFormatException();
      if (result.Errors.Any(error => error.Code == "InvalidUserNameLength")) throw new UserNameLengthException();

      var passwordValidationErrors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
      throw new PasswordValidationException { ValidationErrors = passwordValidationErrors };
    }

    await userManager.AddClaimAsync(user,
      new Claim(OpenIddictConstants.Claims.Locale, request.Locale.GetLocalizationString()));

    string code = await userManager.GenerateEmailConfirmationTokenAsync(user);
    string url = user.GenerateMailConfirmUrl(request.ConfirmUrl, code, request.ReturnUrl);
    var message = new ConfirmRegistrationEmail { Recipient = request.Email, ConfirmLink = url };
    await publishEndpoint.Publish(new SendEmail { Message = message }, cancellationToken);

    return user;
  }
}
