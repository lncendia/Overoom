using Common.Application.Transactions;

using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;

using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Commands.Create;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.External.Controllers;

/// <summary>
/// Класс, представляющий контроллер для внешних провайдеров аутентификации.
/// </summary>
[SecurityHeaders]
public class ExternalController : Controller
{
  /// <summary>
  /// Предоставляет API для входа пользователя.
  /// </summary>
  private readonly SignInManager<AppUser> _signInManager;

  /// <summary>
  /// Медиатор
  /// </summary>
  private readonly ISender _mediator;

  /// <summary>
  /// Логгер
  /// </summary>
  private readonly ILogger<ExternalController> _logger;

  /// <summary>
  /// Конструктор класса ExternalController.
  /// </summary>
  /// <param name="signInManager">Менеджер входа в систему</param>
  /// <param name="logger">Логгер</param>
  /// <param name="mediator">Медиатор</param>
  public ExternalController(ISender mediator, SignInManager<AppUser> signInManager, ILogger<ExternalController> logger)
  {
    _mediator = mediator;
    _signInManager = signInManager;
    _logger = logger;
  }

  /// <summary>
  /// Инициировать двустороннее обращение к внешнему поставщику аутентификации
  /// </summary>
  [HttpGet]
  [AllowAnonymous]
  public IActionResult Challenge(string? provider, string returnUrl = "/")
  {
    if (string.IsNullOrEmpty(provider)) throw new QueryParameterMissingException(nameof(provider));

    string? redirectUrl = Url.Action("ExternalLoginCallback", "External", new { ReturnUrl = returnUrl });
    AuthenticationProperties properties =
      _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

    return new ChallengeResult(provider, properties);
  }

  /// <summary>
  /// Обрабатывает обратный вызов внешней аутентификации.
  /// </summary>
  /// <param name="returnUrl">URL-адрес возврата после успешной аутентификации.</param>
  /// <returns>Результат действия IActionResult.</returns>
  [HttpGet]
  [Transactional]
  public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(returnUrl);
    ExternalLoginInfo? info = await _signInManager.GetExternalLoginInfoAsync();

    if (info == null)
      throw new ExternalAuthenticationFailureException("Couldn't get information about an external authentication");

    await HttpContext.SignOutAsync(info.AuthenticationProperties);
    AppUser user;
    try
    {
      user = await _mediator.Send(new AuthenticateUserByExternalProviderCommand
      {
        LoginProvider = info.LoginProvider,
        ProviderKey = info.ProviderKey
      });
    }

    catch (UserNotFoundException)
    {
      IRequestCultureFeature? requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
      Localization locale = requestCulture!.RequestCulture.UICulture.Name.GetLocalization();

      user = await _mediator.Send(new CreateUserExternalCommand
      {
        LoginInfo = info,
        Locale = locale
      });
    }

    catch (TwoFactorRequiredException ex)
    {
      bool isRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(ex.User);

      if (isRemembered)
      {
        // Клиент запомнен, поэтому пользователь может войти без 2FA
        user = ex.User;
      }
      else
      {
        var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);
        identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, ex.User.Id.ToString()));
        identity.AddClaim(new Claim(Constants.Claims.IdentityProvider, info.LoginProvider));
        await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, new ClaimsPrincipal(identity));

        return RedirectToAction("LoginTwoStep", "TwoFactor", new { returnUrl, rememberMe = true });
      }
    }

    await SignInExternal(user, info, context);

    return Redirect(returnUrl);
  }

  /// <summary>
  /// Асинхронный метод для входа через внешний провайдер аутентификации.
  /// </summary>
  /// <param name="user">Пользователь</param>
  /// <param name="info">Информация о внешнем провайдере аутентификации.</param>
  /// <param name="context">Контекст авторизации.</param>
  /// <returns>Задача, представляющая асинхронную операцию.</returns>
  private async Task SignInExternal(AppUser user, UserLoginInfo info, OpenIddictRequest? context)
  {
    await _signInManager.SignInAsync(user, true, info.LoginProvider);

    _logger.LogInformation(
      "User login successful. Email: {Email}, UserId: {UserId}, UserName: {UserName}, ClientId: {ClientId}",
      user.Email, user.Id, user.UserName, context?.ClientId);
  }
}
