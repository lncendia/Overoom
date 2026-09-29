using System.Security.Claims;
using System.Web;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Commands.TwoFactor;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Queries;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.TwoFactor.InputModels;
using Identix.Infrastructure.Web.TwoFactor.ViewModels;
using Identix.Infrastructure.Web.Extensions;

using OpenIddict.Abstractions;

namespace Identix.Infrastructure.Web.TwoFactor.Controllers;

/// <summary>
/// Контроллер для изменения настроек аккаунта
/// </summary>
[SecurityHeaders]
public class TwoFactorController : Controller
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
  private readonly IStringLocalizer<TwoFactorController> _localizer;

  /// <summary>
  /// Логгер
  /// </summary>
  private readonly ILogger<TwoFactorController> _logger;

  /// <summary>
  /// Конструктор контроллера для прохождения аутентификации.
  /// </summary>
  /// <param name="mediator">Медиатор</param>
  /// <param name="signInManager">Предоставляет API для входа пользователя.</param>
  /// <param name="localizer">Локализатор</param>
  /// <param name="logger">Логгер</param>
  public TwoFactorController(ISender mediator, SignInManager<AppUser> signInManager,
    IStringLocalizer<TwoFactorController> localizer, ILogger<TwoFactorController> logger)
  {
    _mediator = mediator;
    _signInManager = signInManager;
    _localizer = localizer;
    _logger = logger;
  }

  /// <summary>
  /// Точка входа на страницу подключения 2FA
  /// </summary>
  /// <param name="returnUrl">Адрес Url переадресации</param>
  [HttpGet]
  [Authorize]
  public async Task<IActionResult> Setup(string returnUrl = "/")
  {
    SetupTwoFactorViewModel model = await BuildSetupViewModelAsync(returnUrl);

    return View(model);
  }

  /// <summary>
  /// Обработка подключения 2FA
  /// </summary>
  /// <param name="model">Модель подключения 2FA</param>
  [HttpPost]
  [Authorize]
  public async Task<IActionResult> VerifySetup(SetupTwoFactorInputModel model)
  {
    HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

    if (!ModelState.IsValid)
    {
      SetupTwoFactorViewModel viewModel = await BuildSetupViewModelAsync(model.ReturnUrl);
      viewModel.Code = model.Code;

      return View("Setup", viewModel);
    }

    try
    {
      IReadOnlyCollection<string> codes = await _mediator.Send(new VerifySetupTwoFactorTokenCommand
      {
        UserId = User.Id(),
        Code = model.Code!
      });

      return View("VerifySetup", new RecoveryCodesViewModel
      {
        RecoveryCodes = codes,
        ReturnUrl = model.ReturnUrl
      });
    }
    catch (InvalidCodeException)
    {
      ModelState.AddModelError(string.Empty, _localizer["InvalidCode"]);
      SetupTwoFactorViewModel viewModel = await BuildSetupViewModelAsync(model.ReturnUrl);
      viewModel.Code = model.Code;

      return View("Setup", viewModel);
    }
  }

  /// <summary>
  /// Точка входа на прохождение 2FA
  /// </summary>
  /// <param name="rememberMe">Флаг для запоминания пользователя</param>
  /// <param name="returnUrl">Адрес url возврата</param>
  [HttpGet]
  [Authorize(AuthenticationSchemes = "Identity.TwoFactorUserId")]
  public async Task<IActionResult> LoginTwoStep(bool rememberMe, string returnUrl = "/")
  {
    return View(await BuildLoginTwoStepViewModelAsync(rememberMe, returnUrl, CodeType.Authenticator));
  }

  /// <summary>
  /// Обработка прохождения 2FA
  /// </summary>
  /// <param name="model">Модель прохождения 2FA</param>
  [HttpPost]
  [Authorize(AuthenticationSchemes = "Identity.TwoFactorUserId")]
  public async Task<IActionResult> LoginTwoStep(LoginTwoStepInputModel model)
  {
    HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

    if (!ModelState.IsValid)
    {
      ModelState.Clear();

      return View(await BuildLoginTwoStepViewModelAsync(model.RememberMe, model.ReturnUrl, model.CodeType));
    }

    string? loginProvider = User.FindFirstValue(Constants.Claims.IdentityProvider);
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(model.ReturnUrl);

    try
    {
      AppUser user = await _mediator.Send(new AuthenticateTwoFactorCommand
      {
        Code = model.Code!,
        UserId = User.Id(),
        Type = model.CodeType
      });

      await _signInManager.SignInAsync(user, model.RememberMe, loginProvider);
      await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

      if (model.RememberMe)
      {
        await _signInManager.RememberTwoFactorClientAsync(user);
      }

      _logger.LogInformation(
        "User login successful. Email: {Email}, UserId: {UserId}, UserName: {UserName}, ClientId: {ClientId}",
        user.Email, user.Id, user.UserName, context?.ClientId);

      return Redirect(model.ReturnUrl);
    }
    catch (InvalidCodeException)
    {
      ModelState.Clear();
      (AppUser user, ICollection<Claim> _) = await _mediator.Send(new UserByIdQuery { Id = User.Id() });

      _logger.LogWarning(
        "User {Email} failed to login: invalid two-factor code. ClientId: {ClientId}",
        user.Email,
        context?.ClientId);

      ModelState.AddModelError(string.Empty, _localizer["InvalidCode"]);

      return View(await BuildLoginTwoStepViewModelAsync(model.RememberMe, model.ReturnUrl, model.CodeType));
    }
  }

  /// <summary>
  /// Метод отправляет пользователю код для прохождения 2FA на почту
  /// </summary>
  [Authorize(AuthenticationSchemes = "Identity.TwoFactorUserId, Identity.Application")]
  public async Task<IActionResult> RequestCodeEmail()
  {
    try
    {
      await _mediator.Send(new RequestTwoFactorCodeEmailCommand { UserId = User.Id() });
    }
    catch
    {
      return BadRequest();
    }

    return Ok();
  }

  /// <summary>
  /// Точка входа на сброс 2FA
  /// </summary>
  /// <param name="returnUrl">Адрес Url переадресации</param>
  [HttpGet]
  [Authorize]
  public IActionResult Reset(string returnUrl = "/")
  {
    return View(new ResetTwoFactorViewModel { CodeType = CodeType.Authenticator, ReturnUrl = returnUrl });
  }

  /// <summary>
  /// Обработка сброса 2FA
  /// </summary>
  /// <param name="model">Модель сброса 2FA</param>
  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Reset(TwoFactorAuthenticateInputModel model)
  {
    HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

    if (!ModelState.IsValid)
    {
      ModelState.Clear();

      return View(new ResetTwoFactorViewModel { CodeType = model.CodeType, ReturnUrl = model.ReturnUrl });
    }

    try
    {
      AppUser user = await _mediator.Send(new ResetTwoFactorCommand
      {
        UserId = User.Id(),
        Code = model.Code!,
        Type = model.CodeType
      });

      // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
      await _signInManager.RefreshSignInAsync(user);

      return RedirectToAction("Index", "Settings", new { model.ReturnUrl });
    }
    catch (InvalidCodeException)
    {
      ModelState.AddModelError(string.Empty, _localizer["InvalidCode"]);

      return View(new ResetTwoFactorViewModel { CodeType = model.CodeType, ReturnUrl = model.ReturnUrl });
    }
  }

  /// <summary>
  /// Метод формирует модель представления для подключения 2FA
  /// </summary>
  /// <param name="returnUrl">Url для возврата</param>
  /// <returns>Модель представления подключения 2FA</returns>
  private async Task<SetupTwoFactorViewModel> BuildSetupViewModelAsync(string returnUrl)
  {
    (AppUser user, string token) result = await _mediator.Send(new SetupTwoFactorCommand { UserId = User.Id() });
    await _signInManager.RefreshSignInAsync(result.user);

    return new SetupTwoFactorViewModel(result.token, result.user.Email!, "Identix") { ReturnUrl = returnUrl };
  }

  /// <summary>
  /// Метод формирует модель представления для аутентификации через 2FA
  /// </summary>
  /// <param name="rememberMe">Необходимо ли запоминать вход через 2fa</param>
  /// <param name="returnUrl">Url возврата</param>
  /// <param name="codeType">Тип генерации кода</param>
  /// <returns>Модель представления для аутентификации через 2FA</returns>
  private async Task<LoginTwoStepViewModel> BuildLoginTwoStepViewModelAsync(bool rememberMe, string returnUrl,
    CodeType codeType)
  {
    (AppUser user, ICollection<Claim> _) = await _mediator.Send(new UserByIdQuery { Id = User.Id() });

    return new LoginTwoStepViewModel
    {
      CodeType = codeType,
      NeedShowEmail = user.EmailConfirmed,
      RememberMe = rememberMe,
      ReturnUrl = returnUrl
    };
  }
}
