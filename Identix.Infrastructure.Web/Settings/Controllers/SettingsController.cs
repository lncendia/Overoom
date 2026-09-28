using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Commands.External;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Queries;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Settings.InputModels;
using Identix.Infrastructure.Web.Settings.ViewModels;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.Settings.Controllers;

/// <summary>
/// Контроллер для изменения настроек аккаунта
/// </summary>
[Authorize]
[SecurityHeaders]
public class SettingsController : Controller
{
  /// <summary>
  /// Медиатор
  /// </summary>
  private readonly ISender _mediator;

  /// <summary>
  /// Предоставляет API для входа пользователя.
  /// </summary>
  private readonly SignInManager<AppUser> _signInManager;

  /// <summary>
  /// Локализатор
  /// </summary>
  private readonly IStringLocalizer<SettingsController> _localizer;

  /// <summary>
  /// Конструктор контроллера для прохождения аутентификации.
  /// </summary>
  /// <param name="mediator">Медиатор</param>
  /// <param name="signInManager">Предоставляет API для входа пользователя.</param>
  /// <param name="localizer">Локализатор</param>
  public SettingsController(ISender mediator, SignInManager<AppUser> signInManager,
    IStringLocalizer<SettingsController> localizer)
  {
    _mediator = mediator;
    _signInManager = signInManager;
    _localizer = localizer;
  }

  /// <summary>
  /// Страница контроллера по умолчанию
  /// </summary>
  /// <param name="model">Модель данных, необходимых для отображения страницы</param>
  [HttpGet]
  public async Task<IActionResult> Index([FromQuery] SettingsInputModel model)
  {
    (AppUser user, ICollection<Claim> claims) = await _mediator.Send(new UserByIdQuery { Id = User.Id() });
    if (!string.IsNullOrEmpty(model.ErrorMessage)) ModelState.AddModelError("", model.ErrorMessage);

    SettingsViewModel settingsModel = await BuildViewModelAsync(user, claims, model);

    return View(settingsModel);
  }

  /// <summary>
  /// Метод, который вызывается при перенаправлении на внешний провайдер аутентификации для вызова вызова аутентификации.
  /// </summary>
  /// <param name="provider">Имя внешнего провайдера аутентификации</param>
  /// <param name="returnUrl">Url для возврата</param>
  /// <returns>Результат вызова аутентификации</returns>
  [HttpGet]
  public IActionResult Challenge(string? provider, string returnUrl = "/")
  {
    if (string.IsNullOrEmpty(provider)) throw new QueryParameterMissingException(nameof(provider));

    string? redirectUrl = Url.Action("ExternalLoginCallback", "Settings", new { ReturnUrl = returnUrl });
    AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

    return new ChallengeResult(provider, properties);
  }

  /// <summary>
  /// Обрабатывает обратный вызов внешней аутентификации.
  /// </summary>
  /// <param name="returnUrl">Url для возврата</param>
  /// <returns>Результат действия IActionResult.</returns>
  [HttpGet]
  public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
  {
    ExternalLoginInfo? info = await _signInManager.GetExternalLoginInfoAsync();

    if (info == null)
      throw new ExternalAuthenticationFailureException(
        "Couldn't get information about an external authentication");

    AppUser user = await _mediator.Send(new AddUserExternalLoginCommand { UserId = User.Id(), LoginInfo = info });
    await HttpContext.SignOutAsync(info.AuthenticationProperties);

    // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
    await _signInManager.RefreshSignInAsync(user);

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = 1,
      ReturnUrl = returnUrl,
      Message = string.Format(_localizer["ProviderLinked"], info.ProviderDisplayName)
    });
  }

  /// <summary>
  /// Метод, который удаляет вход внешнего провайдера аутентификации у пользователя.
  /// </summary>
  /// <param name="model">Модель с данными для удаления провайдера</param>
  /// <returns>Результат удаления входа</returns>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> RemoveLogin(RemoveLoginInputModel model)
  {
    string? message = null, errorMessage = null;
    if (!ModelState.IsValid) errorMessage = GetFirstError();
    else
    {
      AppUser user = await _mediator.Send(
        new RemoveUserExternalLoginCommand { UserId = User.Id(), Provider = model.Provider! });

      // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
      await _signInManager.RefreshSignInAsync(user);

      string? providerDisplayName = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                                    .FirstOrDefault(p => p.Name == model.Provider)?.DisplayName
                                    ?? model.Provider;

      message = string.Format(_localizer["ProviderUnlinked"], providerDisplayName);
    }

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = 1,
      ReturnUrl = model.ReturnUrl,
      Message = message,
      ErrorMessage = errorMessage
    });
  }

  /// <summary>
  /// Метод, который заканчивает другие сессии у пользователя
  /// </summary>
  /// <param name="model">Модель с данными для закрытия сессий</param>
  /// <returns>Результат закрытия сессий</returns>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> CloseOtherSessions(CloseSessionsInputModel model)
  {
    AppUser user = await _mediator.Send(new UpdateSecurityStampCommand { UserId = User.Id() });

    // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
    await _signInManager.RefreshSignInAsync(user);

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = model.ExpandElement,
      ReturnUrl = model.ReturnUrl,
      Message = _localizer["SessionsClosed"]
    });
  }

  /// <summary>
  /// Метод, который изменяет пароль пользователя.
  /// </summary>
  /// <param name="model">Модель с данными для смены пароля</param>
  /// <returns>Результат смены пароля</returns>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ChangePassword(ChangePasswordInputModel model)
  {
    string? message = null, errorMessage = null;
    if (!ModelState.IsValid) errorMessage = GetFirstError();
    else
    {
      try
      {
        AppUser user = await _mediator.Send(new ChangePasswordCommand(model.OldPassword, model.NewPassword!)
        {
          UserId = User.Id()
        });

        message = _localizer["PasswordChanged"].ToString();

        // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
        await _signInManager.RefreshSignInAsync(user);
      }
      catch (Exception ex)
      {
        switch (ex)
        {
          case PasswordNeededException:
            errorMessage = _localizer["OldPasswordNeeded"];
            break;

          case PasswordValidationException passwordValidationException:
            IEnumerable<LocalizedString> errorsEnumerable = passwordValidationException.ValidationErrors
              .Select(code => _localizer[code.Key]);

            errorMessage = string.Join(", ", errorsEnumerable);
            break;

          case ArgumentException:
            errorMessage = _localizer["OldPasswordMatchNew"];
            break;

          default: throw;
        }
      }
    }

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = 2,
      ReturnUrl = model.ReturnUrl,
      Message = message,
      ErrorMessage = errorMessage
    });
  }

  /// <summary>
  /// Метод для запроса изменения адреса электронной почты пользователя.
  /// </summary>
  /// <param name="model">Модель ввода, содержащая новый адрес электронной почты и другие соответствующие данные.</param>
  /// <returns>Объект IActionResult, представляющий результат операции.</returns>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> RequestChangeEmail(RequestChangeEmailInputModel model)
  {
    string? message = null, errorMessage = null;

    if (!ModelState.IsValid)
    {
      errorMessage = GetFirstError();
    }
    else
    {
      string resetUrl = Url.Action("ChangeEmail", "Settings", null, HttpContext.Request.Scheme)!;

      try
      {
        await _mediator.Send(new RequestChangeEmailCommand
        {
          UserId = User.Id(),
          NewEmail = model.Email!,
          Password = model.Password,
          ResetUrl = resetUrl,
          ReturnUrl = model.ReturnUrl
        });

        message = _localizer["EmailChangeRequested"];
      }
      catch (PasswordNeededException)
      {
        errorMessage = _localizer["PasswordNeeded"];
      }
      catch (InvalidPasswordException)
      {
        errorMessage = _localizer["InvalidPassword"];
      }
    }

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = 3,
      Message = message,
      ReturnUrl = model.ReturnUrl,
      ErrorMessage = errorMessage
    });
  }

  /// <summary>
  /// Метод для изменения адреса электронной почты пользователя.
  /// </summary>
  /// <param name="id">Идентификатор пользователя</param>
  /// <param name="email">Новый адрес электронной почты.</param>
  /// <param name="code">Код подтверждения изменения адреса электронной почты.</param>
  /// <param name="returnUrl">Url для возврата</param>
  /// <returns>Объект IActionResult, представляющий результат операции.</returns>
  [HttpGet]
  public async Task<IActionResult> ChangeEmail(Guid? id, string? email, string? code, string returnUrl = "/")
  {
    if (!id.HasValue || id.Value != User.Id()) throw new QueryParameterMissingException(nameof(id));
    if (string.IsNullOrEmpty(email)) throw new QueryParameterMissingException(nameof(email));
    if (string.IsNullOrEmpty(code)) throw new QueryParameterMissingException(nameof(code));

    string? message = null, errorMessage = null;

    try
    {
      AppUser user = await _mediator.Send(new ChangeEmailCommand
      {
        Code = code,
        NewEmail = email,
        UserId = id.Value
      });

      message = _localizer["EmailChanged"];

      // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
      await _signInManager.RefreshSignInAsync(user);
    }
    catch (Exception ex)
    {
      switch (ex)
      {
        case EmailAlreadyTakenException:
          errorMessage = _localizer["EmailAlreadyTaken"];
          break;

        case EmailFormatException:
          errorMessage = _localizer["EmailFormatInvalid"];
          break;

        default: throw;
      }
    }

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = 3,
      ReturnUrl = returnUrl,
      Message = message,
      ErrorMessage = errorMessage
    });
  }

  /// <summary>
  /// Метод для изменения имени пользователя.
  /// </summary>
  /// <param name="model">Модель с данными смены имени.</param>
  /// <returns>Объект IActionResult, представляющий результат операции.</returns>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ChangeName(ChangeNameInputModel model)
  {
    string? message = null, errorMessage = null;
    if (!ModelState.IsValid) errorMessage = GetFirstError();

    else
    {
      try
      {
        AppUser user = await _mediator.Send(new ChangeNameCommand
        {
          UserId = User.Id(),
          Name = model.Username!
        });

        message = _localizer["UserNameChanged"];

        // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
        await _signInManager.RefreshSignInAsync(user);
      }
      catch (UserNameLengthException)
      {
        errorMessage = _localizer["UserNameLengthInvalid"];
      }
    }

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = 4,
      ReturnUrl = model.ReturnUrl,
      Message = message,
      ErrorMessage = errorMessage
    });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<ActionResult> ChangeAvatar(ChangeAvatarInputModel model)
  {
    string? message = null, errorMessage = null;
    if (!ModelState.IsValid) errorMessage = GetFirstError();
    else if (model.File!.Length > 15728640) errorMessage = _localizer["WrongFileSize"];

    else
    {
      await using Stream stream = model.File!.OpenReadStream();

      await _mediator.Send(new ChangeAvatarCommand
      {
        UserId = User.Id(),

        Thumbnail = stream
      });

      message = _localizer["AvatarChanged"];
    }

    return RedirectToAction("Index", new SettingsInputModel
    {
      ExpandElement = 5,
      ReturnUrl = model.ReturnUrl,
      Message = message,
      ErrorMessage = errorMessage
    });
  }

  /// <summary>
  /// Метод, отвечающий за построение модели представления для страницы настроек.
  /// </summary>
  /// <param name="user">Объект пользователя</param>
  /// <param name="claims">Утверждения пользователя</param>
  /// <param name="model">Модель данных, необходимых для отображения страницы</param>
  /// <returns>Модель представления настроек</returns>
  private async Task<SettingsViewModel> BuildViewModelAsync(AppUser user, ICollection<Claim> claims, SettingsInputModel model)
  {
    IReadOnlyCollection<string> logins = await _mediator.Send(new UserLoginsQuery { Id = user.Id });
    IEnumerable<AuthenticationScheme> schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
    var userSchemes = new List<ExternalProvider>();

    foreach (AuthenticationScheme authenticationScheme in schemes)
    {
      bool isAssociated = logins.Any(login => login == authenticationScheme.Name);

      userSchemes.Add(new ExternalProvider
      {
        DisplayName = authenticationScheme.DisplayName ?? authenticationScheme.Name,
        AuthenticationScheme = authenticationScheme.Name,
        IsAssociated = isAssociated
      });
    }

    string? photoKey = AppUser.GetPhotoKey(claims);

    var settingsModel = new SettingsViewModel
    {
      ReturnUrl = model.ReturnUrl,
      ExternalProviders = userSchemes,
      HasPassword = user.PasswordHash != null,
      ExpandElement = model.ExpandElement,
      Email = user.Email!,
      Message = model.Message,
      TwoFactorEnabled = user.TwoFactorEnabled,
      UserName = user.UserName!,
      Thumbnail = photoKey != null
        ? Url.Action("GetFile", controller: "Photos", new { key = photoKey })
        : null,
    };

    return settingsModel;
  }

  /// <summary>
  /// Получает первую ошибку из ModelState.
  /// </summary>
  /// <returns>Сообщение об ошибке.</returns>
  private string GetFirstError()
  {
    return ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
  }
}
