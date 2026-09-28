using System.Security.Claims;
using System.Web;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Infrastructure.Web.Account.InputModels;
using Identix.Infrastructure.Web.Account.ViewModels;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.Account.Controllers;

/// <summary>
/// Контроллер для прохождения аутентификации.
/// </summary>
[SecurityHeaders]
public class AccountController : Controller
{
  /// <summary>
  /// Предоставляет API для входа пользователя.
  /// </summary>
  private readonly SignInManager<AppUser> _signInManager;

  /// <summary>
  /// Логгер
  /// </summary>
  private readonly ILogger<AccountController> _logger;

  /// <summary>
  /// Локализатор
  /// </summary>
  private readonly IStringLocalizer<AccountController> _localizer;

  /// <summary>
  /// Медиатор
  /// </summary>
  private readonly ISender _mediator;

  /// <summary>
  /// Конструктор контроллера для прохождения аутентификации.
  /// </summary>
  /// <param name="mediator">Медиатор</param>
  /// <param name="signInManager">Предоставляет API для входа пользователя.</param>
  /// <param name="logger">Логгер</param>
  /// <param name="localizer">Локализатор</param>
  public AccountController(ISender mediator, SignInManager<AppUser> signInManager, ILogger<AccountController> logger,
    IStringLocalizer<AccountController> localizer)
  {
    _mediator = mediator;
    _signInManager = signInManager;
    _logger = logger;
    _localizer = localizer;
  }

  /// <summary>
  /// Точка входа на страницу аутентификации
  /// </summary>
  /// <param name="returnUrl">Адрес Url переадресации</param>
  [HttpGet]
  public async Task<IActionResult> Login(string returnUrl = "/")
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(returnUrl);
    LoginViewModel model = await BuildLoginViewModelAsync(returnUrl, context);

    return View(model);
  }

  /// <summary>
  /// Обработка аутентификации
  /// </summary>
  /// <param name="model">Модель входа в систему</param>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Login(LoginInputModel model)
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(model.ReturnUrl);
    HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

    if (!ModelState.IsValid)
    {
      LoginViewModel loginViewModel = await BuildLoginViewModelAsync(model, context);

      return View(loginViewModel);
    }

    try
    {
      string callbackUrl = Url.Action("ConfirmEmail", "Registration", null, HttpContext.Request.Scheme)!;

      AppUser user = await _mediator.Send(new AuthenticateUserByPasswordCommand
      {
        Email = model.Email!,
        Password = model.Password!,
        ConfirmUrl = callbackUrl,
        ReturnUrl = model.ReturnUrl
      });

      await _signInManager.SignInAsync(user, model.RememberLogin);

      _logger.LogInformation(
        "User login successful. Email: {Email}, UserId: {UserId}, UserName: {UserName}, ClientId: {ClientId}",
        user.Email, user.Id, user.UserName, context?.ClientId);

      return Redirect(model.ReturnUrl);
    }
    catch (Exception ex)
    {
      switch (ex)
      {
        case UserNotFoundException:
          ModelState.AddModelError(string.Empty, _localizer["UserNotFound"]);
          break;

        case InvalidPasswordException:
          _logger.LogInformation(
            "User login failed (invalid credentials). Email: {Email}, ClientId: {ClientId}",
            model.Email, context?.ClientId);

          ModelState.AddModelError(string.Empty, _localizer["InvalidCredentials"]);
          break;

        case UserLockoutException:
          _logger.LogInformation("User login failed (user lockout). Email: {Email}, ClientId: {ClientId}",
            model.Email, context?.ClientId);

          ModelState.AddModelError(string.Empty, _localizer["UserLockout"]);
          break;

        case EmailNotConfirmedException:
          return RedirectToAction("ResetPasswordMailSent", new { returnUrl = model.ReturnUrl });

        case TwoFactorRequiredException tfaException:
          bool isRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(tfaException.User);

          if (isRemembered)
          {
            await _signInManager.SignInAsync(tfaException.User, model.RememberLogin);

            _logger.LogInformation(
              "User login successful (2FA). UserName: {UserName}, UserId: {UserId}, ClientId: {ClientId}",
              tfaException.User.UserName, tfaException.User.Id, context?.ClientId);

            return Redirect(model.ReturnUrl);
          }

          var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);
          identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, tfaException.User.Id.ToString()));

          await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme,
            new ClaimsPrincipal(identity));

          return RedirectToAction("LoginTwoStep", "TwoFactor",
            new { returnUrl = model.ReturnUrl, rememberMe = model.RememberLogin });

        default: throw;
      }

      LoginViewModel loginViewModel = await BuildLoginViewModelAsync(model, context);

      return View(loginViewModel);
    }
  }

  /// <summary>
  /// Обрабатывает HTTP GET запрос для сброса пароля.
  /// </summary>
  /// <param name="returnUrl">URL возврата.</param>
  /// <returns>Результат действия для сброса пароля.</returns>
  [HttpGet]
  public IActionResult RecoverPassword(string returnUrl = "/")
  {
    return View(new ResetPasswordInputModel { ReturnUrl = returnUrl });
  }

  /// <summary>
  /// Обрабатывает POST-запрос для сброса пароля.
  /// </summary>
  /// <param name="model">Модель ввода для сброса пароля.</param>
  /// <returns>Результат действия после сброса пароля.</returns>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> RecoverPassword(ResetPasswordInputModel model)
  {
    if (!ModelState.IsValid) return View(model);

    HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

    string url = Url.Action(
      "NewPassword", "Account", new { returnUrl = model.ReturnUrl }, HttpContext.Request.Scheme)!;

    try
    {
      await _mediator.Send(new RequestRecoverPasswordCommand
      {
        Email = model.Email!,
        ResetUrl = url,
        ReturnUrl = model.ReturnUrl
      });
    }
    catch (UserNotFoundException)
    {
      // Игнорируем, чтобы не раскрыть конфиденциальную информацию
    }

    return RedirectToAction("ResetPasswordMailSent", new { returnUrl = model.ReturnUrl });
  }

  /// <summary>
  /// Возвращает представление для страницы "MailSent".
  /// </summary>
  /// <returns>Результат действия для страницы "MailSent".</returns>
  public IActionResult ConfirmEmailMailSent(string returnUrl = "/")
  {
    return View("MailSent", new MailSentViewModel(_localizer.GetString("MailSent_ConfirmEmail"), returnUrl));
  }

  /// <summary>
  /// Возвращает представление для страницы "ResetPasswordMailSent".
  /// </summary>
  /// <returns>Результат действия для страницы "MailSent".</returns>
  public IActionResult ResetPasswordMailSent(string returnUrl = "/")
  {
    return View("MailSent", new MailSentViewModel(_localizer.GetString("MailSent_ResetPassword"), returnUrl));
  }

  /// <summary>
  /// Обрабатывает HTTP GET запрос для установки нового пароля.
  /// </summary>
  /// <param name="id">Идентификатор пользователя.</param>
  /// <param name="code">Параметр code.</param>
  /// <param name="returnUrl">URL возврата.</param>
  /// <returns>Результат действия для установки нового пароля.</returns>
  [HttpGet]
  public IActionResult NewPassword(Guid? id, string? code, string returnUrl = "/")
  {
    if (!id.HasValue) throw new QueryParameterMissingException(nameof(id));
    if (string.IsNullOrEmpty(code)) throw new QueryParameterMissingException(nameof(code));

    return View(new NewPasswordInputModel { UserId = id.Value, Code = code, ReturnUrl = returnUrl });
  }

  /// <summary>
  /// Обрабатывает POST-запрос на сброс нового пароля.
  /// </summary>
  /// <param name="model">Модель ввода для нового пароля.</param>
  /// <returns>Результат действия после сброса пароля.</returns>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> NewPassword(NewPasswordInputModel model)
  {
    HttpContext.Request.QueryString =
      new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl) + "&Id=" +
                      HttpUtility.UrlEncode(model.UserId.ToString()) + "&Code=" +
                      HttpUtility.UrlEncode(model.Code));

    if (!ModelState.IsValid) return View(model);

    try
    {
      await _mediator.Send(new RecoverPasswordCommand
      {
        Code = model.Code!,
        UserId = model.UserId,
        NewPassword = model.NewPassword!
      });

      return RedirectToAction("Login", new { returnUrl = model.ReturnUrl });
    }
    catch (PasswordValidationException ex)
    {
      foreach (KeyValuePair<string, string> error in ex.ValidationErrors)
      {
        ModelState.AddModelError("", _localizer[error.Key]);
      }
    }

    return View(model);
  }

  /// <summary>
  /// Метод обрабатывает нажатие кнопки "Отмена", производит редирект
  /// </summary>
  /// <param name="returnUrl">Адрес Url переадресации </param>
  /// <returns></returns>
  [HttpGet]
  [AllowAnonymous]
  public IActionResult Cancel(string returnUrl = "/")
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(returnUrl);

    if (context == null)
      return Redirect(returnUrl);

    HttpContext.Session.DenyConsent(context, User.GetId());

    return Redirect(returnUrl);
  }

  /// <summary>
  /// Показать страницу выхода
  /// </summary>
  [HttpGet]
  public IActionResult Logout(string returnUrl = "/")
  {
    var inputModel = new LogoutInputModel { ReturnUrl = returnUrl };

    return View(inputModel);
  }

  /// <summary>
  /// Обработка постбэка страницы выхода
  /// </summary>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Logout(LogoutInputModel model)
  {
    if (User.Identity?.IsAuthenticated != true)
    {
      return Redirect(model.ReturnUrl);
    }

    await _signInManager.SignOutAsync();
    _logger.LogInformation("User logout successful. Id: {Id}", User.Id());

    return Redirect(model.ReturnUrl);
  }

  /// <summary>
  /// Создает модель представления входа
  /// </summary>
  /// <param name="returnUrl">url возврата</param>
  /// <param name="context">Контекст авторизации</param>
  /// <returns>Модель представления входа</returns>
  private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl, OpenIddictRequest? context)
  {
    IEnumerable<string> schemes = (await _signInManager.GetExternalAuthenticationSchemesAsync()).Select(s => s.Name);
    bool enableLocalIdentityProvider = true;

    if (context == null)
    {
      return new LoginViewModel
      {
        ReturnUrl = returnUrl,

        EnableLocalLogin = enableLocalIdentityProvider,

        ExternalProviders = [.. schemes]
      };
    }

    if (context.IdentityProvider != null)
    {
      if (context.IdentityProvider == Constants.IdentityProviders.Local)
      {
        schemes = [];
      }
      else
      {
        enableLocalIdentityProvider = false;
        schemes = schemes.Where(provider => provider == context.IdentityProvider);
      }
    }

    return new LoginViewModel
    {
      ReturnUrl = returnUrl,

      EnableLocalLogin = enableLocalIdentityProvider,

      ExternalProviders = [.. schemes],

      Email = context.LoginHint
    };
  }

  /// <summary>
  /// Построить асинхронную модель представления входа
  /// </summary>
  /// <param name="model">Модель, прилетевшая в контроллер</param>
  /// <param name="context">Контекст авторизации</param>
  /// <returns></returns>
  private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model, OpenIddictRequest? context)
  {
    LoginViewModel vm = await BuildLoginViewModelAsync(model.ReturnUrl, context);
    vm.Email = model.Email;
    vm.RememberLogin = model.RememberLogin;
    vm.Password = model.Password;

    return vm;
  }
}