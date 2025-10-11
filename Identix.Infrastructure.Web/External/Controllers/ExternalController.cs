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
        // Проверяем, является ли `provider` пустым или `returnUrl` недействительным
        if (string.IsNullOrEmpty(provider)) throw new QueryParameterMissingException(nameof(provider));

        // Создаем URL-адрес для перенаправления на действие "ExternalLoginCallback" контроллера "External" с параметром "ReturnUrl"
        var redirectUrl = Url.Action("ExternalLoginCallback", "External", new { ReturnUrl = returnUrl });

        // Настраиваем свойства аутентификации для внешней аутентификации с использованием `provider` и `redirectUrl`
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        // Возвращаем результат вызова аутентификации ChallengeResult с указанным `provider` и `properties`
        return new ChallengeResult(provider, properties);
    }

    /// <summary>
    /// Обрабатывает обратный вызов внешней аутентификации.
    /// </summary>
    /// <param name="returnUrl">URL-адрес возврата после успешной аутентификации.</param>
    /// <returns>Результат действия IActionResult.</returns>
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
    {
        // Проверяем, находимся ли мы в контексте запроса авторизации
        var context = HttpContext.Session.GetOpenIdRequest(returnUrl);

        // Получаем информацию о внешней аутентификации
        var info = await _signInManager.GetExternalLoginInfoAsync();

        // Если информация о внешнем провайдере недоступна, прерываем процесс аутентификации
        if (info == null) throw new ExternalAuthenticationFailureException("Couldn't get information about an external authentication");
        
        // Отчищаем куки данных от внешнего провайдера
        await HttpContext.SignOutAsync(info.AuthenticationProperties);

        // Создаем переменную для данных пользователя
        AppUser user;
        try
        {
            // Пробуем аутентифицировать пользователя по внешнему логину
            user = await _mediator.Send(new AuthenticateUserByExternalProviderCommand
            {
                // Провайдер аутентификации
                LoginProvider = info.LoginProvider,

                // Ключ провайдера аутентификации
                ProviderKey = info.ProviderKey
            });
        }

        // Если пользователь не найден - регистрируем его
        catch (UserNotFoundException)
        {
            // Получаем сервис IRequestCultureFeature
            var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();

            // Получаем текущую локаль
            var locale = requestCulture!.RequestCulture.UICulture.Name.GetLocalization();

            // Отправляем команду на создание пользователя по данным внешнего логина
            user = await _mediator.Send(new CreateUserExternalCommand
            {
                // Данные от внешнего провайдера
                LoginInfo = info,

                // Локаль пользователя
                Locale = locale
            });
        }

        catch (TwoFactorRequiredException ex)
        {
            // Получаем, был ли запомнен пользователь системой 2FA
            var isRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(ex.User);

            // Если пользователь был запомнен
            if (isRemembered)
            {
                // Устанавливаем пользователя из исключения и прерываем обработку исключения (так как пользователь может быть авторизован без 2fa)
                user = ex.User;
            }
            else
            {
                // Формируем объект ClaimsIdentity на основе схемы 2FA
                var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);

                // Добавляем новый Claim на основе имени пользователя
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, ex.User.Id.ToString()));
                
                // Добавляем новый Claim на основе idp
                identity.AddClaim(new Claim(Constants.Claims.IdentityProvider, info.LoginProvider));

                // Осуществляем вход пользователя по схеме 2FA 
                await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, new ClaimsPrincipal(identity));

                // Перенаправляем пользователя на страницу прохождения 2FA
                return RedirectToAction("LoginTwoStep", "TwoFactor", new { returnUrl, rememberMe = true });
            }
        }

        // Выполняем вход пользователя через внешнюю аутентификацию.
        await SignInExternal(user, info, context);

        // Перенаправляем по url возврата
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
        // Выполняем асинхронный вход пользователя.
        await _signInManager.SignInAsync(user, true, info.LoginProvider);

        // Инициализируем событие об успешном входе пользователя
        _logger.LogInformation(
            "User login successful. Email: {Email}, UserId: {UserId}, UserName: {UserName}, ClientId: {ClientId}",
            user.Email, user.Id, user.UserName, context?.ClientId);
    }
}