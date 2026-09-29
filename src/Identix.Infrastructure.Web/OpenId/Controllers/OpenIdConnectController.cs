using System.Security.Claims;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Commands.OpenId;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Queries;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Identix.Infrastructure.Web.OpenId.Controllers;

/// <summary>
/// Контроллер для обработки запросов OpenID Connect (OIDC)
/// </summary>
public class OpenIdConnectController : Controller
{
  #region Сообщения ошибок

  private const string LoginRequired = "The user is not authenticated.";
  private const string ExternalConsentRequired = "External consent is required to access this application.";
  private const string InteractiveConsentRequired = "Interactive user consent is required.";
  private const string EmailNotConfirmed = "Email address has not been verified. The confirmation link has been sent.";
  private const string UserNotFound = "User not found.";
  private const string InvalidPassword = "Invalid password.";
  private const string AccountLocked = "Account is locked.";
  private const string TwoFactorRequired = "Two-factor authentication is required.";

  #endregion

  /// <summary>
  /// Медиатор для обработки CQRS запросов и команд
  /// </summary>
  private readonly ISender _mediator;

  /// <summary>
  /// Инициализирует новый экземпляр контроллера OpenID Connect
  /// </summary>
  /// <param name="mediator">Медиатор для обработки запросов (внедряется через DI)</param>
  public OpenIdConnectController(ISender mediator)
  {
    _mediator = mediator;
  }

  /// <summary>
  /// Обработчик endpoint'а авторизации OIDC
  /// Поддерживает как GET, так и POST запросы
  /// </summary>
  /// <param name="cancellationToken">Токен отмены операции</param>
  /// <returns>Результат авторизации (редирект, ошибка или форма согласия)</returns>
  [HttpGet("~/connect/authorize")]
  [HttpPost("~/connect/authorize")]
  [IgnoreAntiforgeryToken]
  public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
  {
    OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new OpenIdContextException();
    AuthenticateResult result = await HttpContext.AuthenticateAsync();
    OpenIdExtensions.ConsentResponse? consent = HttpContext.Session.TakeConsent(request, result.Principal.GetId());

    if (consent is { IsGranted: false })
    {
      return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    if (RequiresLogin(request, result))
    {
      return HandleLoginRequired(request);
    }

    if (consent is { IsGranted: true })
    {
      return await HandleGrantedConsent(consent, request, cancellationToken);
    }

    if (request.HasPromptValue(OpenIddictConstants.PromptValues.Consent))
    {
      return RedirectToConsent(request);
    }

    return await HandleAuthorization(request, cancellationToken);
  }

  /// <summary>
  /// Эндпоинт OpenID Connect для выдачи токенов (Token Endpoint)
  /// Обрабатывает различные типы grant flow согласно спецификации OAuth 2.0/OpenID Connect
  /// </summary>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>Access/Refresh токены в формате JSON</returns>
  /// <exception cref="InvalidOperationException">Выбрасывается при неподдерживаемом grant type</exception>
  [HttpPost("~/connect/token")]
  [Produces("application/json")]
  [IgnoreAntiforgeryToken]
  public async Task<IActionResult> Exchange(CancellationToken token)
  {
    OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new OpenIdContextException();

    if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
      return await HandleCodeOrRefreshAsync(token);

    if (request.IsPasswordGrantType())
      return await HandlePasswordAsync(request, token);

    if (request.IsClientCredentialsGrantType())
      return await HandleClientCredentialsAsync(request, token);

    throw new InvalidOperationException($"Unsupported grant type: {request.GrantType}");
  }

  /// <summary>
  /// Точка выхода пользователя (эндпоинт OpenIddict).
  /// </summary>
  /// <returns>Результат выхода из системы.</returns>
  [HttpGet("~/connect/logout")]
  public IActionResult Logout()
  {
    if (User.Identity?.IsAuthenticated == true)
    {
      return RedirectToAction("Logout", "Account",
        new { returnUrl = Request.PathBase + Request.Path + QueryString.Create(Request.Query) });
    }

    return SignOut(
      authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
      properties: new AuthenticationProperties { RedirectUri = "/" });
  }

  /// <summary>
  /// Эндпоинт OpenID Connect для получения информации о пользователе (UserInfo)
  /// </summary>
  /// <param name="token">Токен отмены операции</param>
  /// <returns>JSON с набором claims пользователя</returns>
  [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
  [HttpGet("~/connect/userinfo")]
  [HttpPost("~/connect/userinfo")]
  [Produces("application/json")]
  public async Task<IActionResult> UserInfo(CancellationToken token)
  {
    var query = new UserInfoQuery
    {
      UserId = User.Id(),
      Scopes = User.GetScopes()
    };

    UserInfoDto claims = await _mediator.Send(query, token);

    return Ok(claims);
  }

  #region Приватные методы

  /// <summary>
  /// Определяет, требуется ли принудительная повторная аутентификация пользователя
  /// </summary>
  /// <param name="request">Запрос OpenID Connect</param>
  /// <param name="result">Результат предыдущей аутентификации</param>
  /// <returns>true - если пользователь должен повторно войти в систему</returns>
  private static bool RequiresLogin(OpenIddictRequest request, AuthenticateResult result)
  {
    return
      result is not { Succeeded: true } ||

      request.HasPromptValue(OpenIddictConstants.PromptValues.Login) ||

      request.MaxAge == 0 ||

      (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
       TimeProvider.System.GetUtcNow() - result.Properties.IssuedUtc >
       TimeSpan.FromSeconds(request.MaxAge.Value));
  }

  /// <summary>
  /// Обрабатывает сценарий, когда требуется аутентификация пользователя
  /// </summary>
  /// <param name="request">OIDC запрос</param>
  /// <returns>Редирект на страницу логина или ошибку</returns>
  private IActionResult HandleLoginRequired(OpenIddictRequest request)
  {
    if (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
    {
      return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
          [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
          [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = LoginRequired
        }));
    }

    HttpContext.Session.SetOpenIdRequest(request);

    return Challenge(new AuthenticationProperties
    {
      RedirectUri = HttpContext.Request.GetEncodedUrl()
    });
  }

  /// <summary>
  /// Обрабатывает сценарий, когда согласие уже предоставлено
  /// </summary>
  /// <param name="consent">Объект с информацией о согласии</param>
  /// <param name="request">OIDC запрос</param>
  /// <param name="cancellationToken">Токен отмены</param>
  /// <returns>Результат с подписанным токеном</returns>
  private async Task<IActionResult> HandleGrantedConsent(OpenIdExtensions.ConsentResponse consent,
    OpenIddictRequest request, CancellationToken cancellationToken)
  {
    var grantCommand = new GrantConsentCommand
    {
      UserId = User.Id(), // ID текущего пользователя
      ClientId = request.ClientId!, // ID OAuth клиента (приложения)
      RememberConsent = consent.RememberConsent, // Флаг "запомнить решение"
      Scopes = [..consent.GrantedScopes], // Разрешенные scope'ы
      Identity = User.Identities.First(), // Текущий identity
      Description = consent.Description, // Описание разрешения
      AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
    };

    ClaimsPrincipal principal = await _mediator.Send(grantCommand, cancellationToken);

    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  /// <summary>
  /// Основной обработчик авторизации - проверяет требования и выдает токены
  /// </summary>
  /// <param name="request">OIDC запрос</param>
  /// <param name="cancellationToken">Токен отмены</param>
  /// <returns>Результат авторизации или редирект на согласие</returns>
  private async Task<IActionResult> HandleAuthorization(OpenIddictRequest request,
    CancellationToken cancellationToken)
  {
    var command = new AuthorizeUserCommand
    {
      UserId = User.Id(), // ID текущего пользователя
      ClientId = request.ClientId!, // ID клиентского приложения
      Scopes = request.GetScopes(), // Запрошенные scope'ы из OIDC запроса
      Identity = User.Identities.First(), // Текущий identity
      AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
    };

    try
    {
      ClaimsPrincipal principal = await _mediator.Send(command, cancellationToken);

      return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    catch (ConsentRequiredException ex) when (ex.ConsentType == OpenIddictConstants.ConsentTypes.External)
    {
      return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
          [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
          [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = ExternalConsentRequired
        }));
    }
    catch (ConsentRequiredException) when (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
    {
      return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
          [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
          [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = InteractiveConsentRequired
        }));
    }
    catch (ConsentRequiredException)
    {
      return RedirectToConsent(request);
    }
  }

  /// <summary>
  /// Перенаправляет пользователя на страницу согласия (consent form)
  /// </summary>
  /// <param name="request">OIDC запрос с параметрами авторизации</param>
  /// <returns>Редирект на страницу согласия</returns>
  private RedirectToActionResult RedirectToConsent(OpenIddictRequest request)
  {
    HttpContext.Session.SetOpenIdRequest(request);

    return RedirectToAction("Index", "Consent",
      new { returnUrl = HttpContext.Request.GetEncodedUrl() });
  }

  /// <summary>
  /// Обрабатывает запросы на обновление токена доступа (refresh token) или аутентификацию по коду авторизации.
  /// Выполняет повторную аутентификацию существующего principal и обновляет его claims.
  /// </summary>
  /// <param name="token">Токен отмены для асинхронной операции</param>
  private async Task<IActionResult> HandleCodeOrRefreshAsync(CancellationToken token)
  {
    // В случае refresh token flow здесь будет principal из refresh token
    // В случае authorization code flow здесь будет principal из кода авторизации
    AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    var command = new RefreshPrincipalCommand
    {
      UserId = result.Principal!.Id(), // ID пользователя из аутентифицированного principal
      Identity = result.Principal!.Identities.First(), // Текущий identity
      AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
    };

    ClaimsPrincipal principal = await _mediator.Send(command, token);

    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  /// <summary>
  /// Обрабатывает запрос аутентификации по паролю в рамках протокола OpenID Connect.
  /// Выполняет проверку учетных данных и создает principal для аутентифицированного пользователя.
  /// </summary>
  /// <param name="request">Запрос OpenID Connect, содержащий параметры аутентификации</param>
  /// <param name="token">Токен отмены для асинхронной операции</param>
  /// <exception cref="Exception">Пробрасывает непредусмотренные исключения наверх</exception>
  private async Task<IActionResult> HandlePasswordAsync(OpenIddictRequest request, CancellationToken token)
  {
    try
    {
      string callbackUrl = Url.Action("ConfirmEmail", "Registration", null, HttpContext.Request.Scheme)!;

      var authenticateCommand = new AuthenticateUserByPasswordCommand
      {
        Email = request.Username!,
        Password = request.Password!,
        ConfirmUrl = callbackUrl
      };

      AppUser user = await _mediator.Send(authenticateCommand, token);
      string time = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds().ToString();

      var identity = new ClaimsIdentity(
        [
          new Claim(Constants.Claims.IdentityProvider, Constants.IdentityProviders.Local),
          new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference, Constants.AuthenticationMethods.Password),
          new Claim(OpenIddictConstants.Claims.AuthenticationTime, time, ClaimValueTypes.Integer64)
        ],
        authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        nameType: OpenIddictConstants.Claims.Name,
        roleType: OpenIddictConstants.Claims.Role);

      var authorizeCommand = new AuthorizeUserCommand
      {
        UserId = user.Id, // ID текущего пользователя
        ClientId = request.ClientId!, // ID клиентского приложения
        Scopes = request.GetScopes(), // Запрашиваемые scope'ы доступа
        Identity = identity, // Текущий identity
        AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
      };

      ClaimsPrincipal principal = await _mediator.Send(authorizeCommand, token);

      return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    catch (Exception ex)
    {
      string? error;
      string? description;

      switch (ex)
      {
        case UserNotFoundException:
          (error, description) = (OpenIddictConstants.Errors.InvalidGrant, UserNotFound);
          break;
        case InvalidPasswordException:
          (error, description) = (OpenIddictConstants.Errors.InvalidGrant, InvalidPassword);
          break;
        case UserLockoutException:
          (error, description) = (OpenIddictConstants.Errors.InvalidGrant, AccountLocked);
          break;
        case TwoFactorRequiredException:
          (error, description) = (OpenIddictConstants.Errors.InteractionRequired, TwoFactorRequired);
          break;
        case ConsentRequiredException { ConsentType: OpenIddictConstants.ConsentTypes.External }:
          (error, description) = (OpenIddictConstants.Errors.ConsentRequired, ExternalConsentRequired);
          break;
        case ConsentRequiredException:
          (error, description) = (OpenIddictConstants.Errors.ConsentRequired, InteractiveConsentRequired);
          break;
        case EmailNotConfirmedException:
          (error, description) = (OpenIddictConstants.Errors.InteractionRequired, EmailNotConfirmed);
          break;
        default:
          throw;
      }

      return Forbid(
        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
          [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
          [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
        }));
    }
  }


  /// <summary>
  /// Обрабатывает запрос аутентификации клиентского приложения в рамках протокола OpenID Connect.
  /// Выполняет проверку учетных данных и создает principal для аутентифицированного клиентского приложения.
  /// </summary>
  /// <param name="request">Запрос OpenID Connect, содержащий параметры клиентского приложения</param>
  /// <param name="token">Токен отмены для асинхронной операции</param>
  private async Task<IActionResult> HandleClientCredentialsAsync(OpenIddictRequest request, CancellationToken token)
  {
    var authorizeCommand = new AuthorizeClientCommand
    {
      ClientId = request.ClientId!,
      Scopes = request.GetScopes()
    };

    ClaimsPrincipal principal = await _mediator.Send(authorizeCommand, token);

    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
  }

  #endregion
}