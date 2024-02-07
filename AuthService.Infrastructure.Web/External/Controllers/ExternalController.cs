using System.Security.Claims;
using AuthService.Application.Abstractions.Commands.Authentication;
using AuthService.Application.Abstractions.Commands.Create;
using AuthService.Application.Abstractions.Entities;
using AuthService.Application.Abstractions.Exceptions;
using AuthService.Infrastructure.Web.Exceptions;
using IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Infrastructure.Web.External.Controllers;

/// <summary>
/// Класс, представляющий контроллер для внешних провайдеров аутентификации.
/// </summary>
[AllowAnonymous]
public class ExternalController(IMediator mediator, SignInManager<UserData> signInManager) : Controller
{
    /// <summary>
    /// инициировать двустороннее обращение к внешнему поставщику аутентификации
    /// </summary>
    [HttpGet]
    public IActionResult Challenge(string? provider, string returnUrl = "/")
    {
        // Проверяем, является ли `provider` пустым или `returnUrl` недействительным
        if (string.IsNullOrEmpty(provider)) throw new QueryParameterMissingException(nameof(provider));

        // Создаем URL-адрес для перенаправления на действие "ExternalLoginCallback" контроллера "External" с параметром "ReturnUrl"
        var redirectUrl = Url.Action("ExternalLoginCallback", "External", new { ReturnUrl = returnUrl });

        // Настраиваем свойства аутентификации для внешней аутентификации с использованием `provider` и `redirectUrl`
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        // Возвращаем результат вызова вызова аутентификации ChallengeResult с указанным `provider` и `properties`
        return new ChallengeResult(provider, properties);
    }

    /// <summary>
    /// Обрабатывает обратный вызов внешней аутентификации.
    /// </summary>
    /// <param name="returnUrl">URL-адрес возврата после успешной аутентификации.</param>
    /// <returns>Результат действия IActionResult.</returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Identity.External")]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
    {
        // Получаем информацию о внешней аутентификации.
        var info = await signInManager.GetExternalLoginInfoAsync();
        
        // Отчищаем куки данных от внешнего провайдера
        await HttpContext.SignOutAsync(info!.AuthenticationProperties);

        // Создаем переменную для данных пользователя
        UserData user;
        try
        {
            // Пробуем аутентифицировать пользователя по внешнему логину
            user = await mediator.Send(new AuthenticateUserByExternalProviderCommand
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
            // Отправляем команду на создание пользователя по данным внешнего логина
            user = await mediator.Send(new CreateUserExternalCommand { LoginInfo = info });
        }

        catch (TwoFactorRequiredException ex)
        {
            // Получаем, был ли запомнен пользователь системой 2FA
            var isRemebered = await signInManager.IsTwoFactorClientRememberedAsync(ex.User);

            // Если пользователь был запомнен
            if (isRemebered)
            {
                // Устанавливаем пользователя из исключения и прерываем обработку исключения (так как пользователь может быть авторизован без 2fa)
                user = ex.User;
            }
            else
            {
                // Формируем объект ClaimsIdentity на основе схемы 2FA
                var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);

                // Добавляем новый Claim на основе имени пользователя
                identity.AddClaim(new Claim(JwtClaimTypes.Subject, ex.User.Id.ToString()));
                
                // Добавляем новый Claim на основе idp
                identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, info.LoginProvider));

                // Осуществляем вход пользователя по схеме 2FA 
                await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, new ClaimsPrincipal(identity));

                // Перенаправляем пользователя на страницу прохождения 2FA
                return RedirectToAction("LoginTwoStep", "TwoFactor", new { returnUrl, rememberMe = true });
            }
        }

        // Выполняем вход пользователя через внешнюю аутентификацию.
        await signInManager.SignInAsync(user, true, info.LoginProvider);

        // Перенаправляем по url возврата
        return Redirect(returnUrl);
    }
}