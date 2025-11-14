using System.Security.Claims;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Commands.OpenId;
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
[ApiController]
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
        // Получаем OIDC запрос из контекста HTTP
        var request = HttpContext.GetOpenIddictServerRequest() ?? throw new OpenIdContextException();

        // Проверяем аутентификацию пользователя (если есть активная сессия)
        var result = await HttpContext.AuthenticateAsync();

        // Получаем сохраненное согласие пользователя из сессии (если есть)
        var consent = HttpContext.Session.TakeConsent(request, result.Principal.GetId());

        // Если пользователь ранее явно отказал в согласии (IsGranted = false)
        if (consent is { IsGranted: false })
        {
            // Немедленно возвращаем отказ в авторизации
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Проверяем, требуется ли повторная аутентификация пользователя
        if (RequiresLogin(request, result))
        {
            // Перенаправляем на страницу логина если требуется аутентификация
            return HandleLoginRequired(request);
        }

        // Если согласие уже было предоставлено ранее
        if (consent is { IsGranted: true })
        {
            // Обрабатываем успешное согласие и выдаем токен
            return await HandleGrantedConsent(consent, request, cancellationToken);
        }

        // Если требуется явное согласие
        if (request.HasPromptValue(OpenIddictConstants.PromptValues.Consent))
        {
            // Перенаправляем на страницу согласия
            return RedirectToConsent(request);
        }

        // Если согласие еще не запрашивалось или не принято - переходим к основной логике авторизации
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
        // Получение и валидация OpenID Connect запроса
        var request = HttpContext.GetOpenIddictServerRequest() ?? throw new OpenIdContextException();

        // Обработка Authorization Code и Refresh Token flow
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            return await HandleCodeOrRefreshAsync(token);

        // Обработка Resource Owner Password Credentials flow
        if (request.IsPasswordGrantType())
            return await HandlePasswordAsync(request, token);

        // Обработка Client Credentials flow
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
        // Если пользователь авторизован
        if (User.Identity?.IsAuthenticated == true)
        {
            // Перенаправляем в AccountController.Logout, чтобы выполнить прикладную логику выхода
            return RedirectToAction("Logout", "Account",
                new { returnUrl = Request.PathBase + Request.Path + QueryString.Create(Request.Query) });
        }

        // Если пользователь уже не авторизован — инициируем завершение сессии на стороне OpenIddict
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
        // Формируем запрос для получения информации о пользователе
        var query = new UserInfoQuery
        {
            UserId = User.Id(),
            Scopes = User.GetScopes()
        };

        // Получаем набор claims через медиатор
        var claims = await _mediator.Send(query, token);

        // Возвращаем JSON с данными пользователя
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
        // Последовательная проверка условий, требующих повторной аутентификации
        return
            // Базовая проверка успешности аутентификации
            result is not { Succeeded: true } ||

            // Явное требование повторного входа (prompt=login)
            request.HasPromptValue(OpenIddictConstants.PromptValues.Login) ||

            // Требование немедленной аутентификации (max_age=0)
            request.MaxAge == 0 ||

            // Проверка истечения времени сессии
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
        // Если запрос содержит prompt=none (тихий режим без взаимодействия)
        if (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
        {
            // Возвращаем ошибку вместо редиректа на логин
            return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = LoginRequired
                }));
        }

        // Сохраняем OIDC запрос в сессии для восстановления после аутентификации
        HttpContext.Session.SetOpenIdRequest(request);

        // Перенаправляем на страницу логина с сохранением текущего URL
        return Challenge(new AuthenticationProperties
        {
            // URL для возврата после успешной аутентификации
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
        // Создаем команду для выдачи согласия через Mediator pattern
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

        // Отправляем команду через медиатор для обработки бизнес-логики
        var principal = await _mediator.Send(grantCommand, cancellationToken);

        // Возвращаем подписанный OIDC токен (id_token или access_token)
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
        // Создаем команду авторизации пользователя
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
            // Пытаемся авторизовать пользователя через медиатор
            var principal = await _mediator.Send(command, cancellationToken);

            // Если успешно - возвращаем подписанный токен
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        // Обрабатываем исключение "требуется внешнее согласие"
        catch (ConsentRequiredException ex) when (ex.ConsentType == OpenIddictConstants.ConsentTypes.External)
        {
            // Возвращаем ошибку для внешнего согласия (не требующего UI)
            return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = ExternalConsentRequired
                }));
        }
        // Обрабатываем исключение когда требуется согласие, но запрос в тихом режиме (prompt=none)
        catch (ConsentRequiredException) when (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
        {
            // Возвращаем ошибку вместо показа формы согласия
            return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = InteractiveConsentRequired
                }));
        }
        // Обрабатываем общий случай требования согласия
        catch (ConsentRequiredException)
        {
            // Перенаправляем на страницу согласия
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
        // Сохраняем OIDC запрос в сессии для формы согласия
        HttpContext.Session.SetOpenIdRequest(request);

        // Перенаправляем на страницу согласия
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
        // Аутентификация существующего principal из текущего HTTP-контекста
        // В случае refresh token flow здесь будет principal из refresh token
        // В случае authorization code flow здесь будет principal из кода авторизации
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Команда для обновления principal с актуальными claims
        var command = new RefreshPrincipalCommand
        {
            UserId = result.Principal!.Id(), // ID пользователя из аутентифицированного principal
            Identity = result.Principal!.Identities.First(), // Текущий identity
            AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        };

        // Выполнение команды обновления principal через Mediator
        var principal = await _mediator.Send(command, token);

        // Возврат результата с обновленным principal
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
            // Создает URL-адрес для подтверждения почты
            var callbackUrl = Url.Action("ConfirmEmail", "Registration", null, HttpContext.Request.Scheme)!;
            
            // Аутентификация пользователя по email и паролю
            var authenticateCommand = new AuthenticateUserByPasswordCommand
            {
                Email = request.Username!,
                Password = request.Password!,
                ConfirmUrl = callbackUrl
            };

            // Выполнение команды аутентификации через Mediator
            var user = await _mediator.Send(authenticateCommand, token);

            // Преобразуем DateTime в Unix timestamp (секунды с 1970-01-01)
            var time = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds().ToString();

            // Создание базовой identity с информацией об аутентификации
            var identity = new ClaimsIdentity(
                [
                    new Claim(Constants.Claims.IdentityProvider, Constants.IdentityProviders.Local),
                    new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference, Constants.AuthenticationMethods.Password),
                    new Claim(OpenIddictConstants.Claims.AuthenticationTime, time, ClaimValueTypes.Integer64)
                ],
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);
            
            // Создание principal для аутентифицированного пользователя
            var authorizeCommand = new AuthorizeUserCommand
            {
                UserId = user.Id, // ID текущего пользователя
                ClientId = request.ClientId!, // ID клиентского приложения
                Scopes = request.GetScopes(), // Запрашиваемые scope'ы доступа
                Identity = identity, // Текущий identity
                AuthenticationScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            };

            // Получение principal с claims для токена
            var principal = await _mediator.Send(authorizeCommand, token);

            // Возврат результата аутентификации
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            // Обработка различных типов исключений с маппингом на стандартные ошибки OpenID Connect
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

            // Возврат ошибки аутентификации в формате OpenID Connect
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
        // Создание principal для аутентифицированного пользователя
        var authorizeCommand = new AuthorizeClientCommand
        {
            ClientId = request.ClientId!,
            Scopes = request.GetScopes()
        };

        // Получение principal с claims для токена
        var principal = await _mediator.Send(authorizeCommand, token);

        // Возврат результата аутентификации
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    #endregion
}