using System.Security.Claims;
using Identix.Infrastructure.Web.External.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identix.Infrastructure.Web.External.Controllers;

/// <summary>
/// Контроллер для обработки колбэков внешней аутентификации
/// </summary>
public class CallbackController : Controller
{
    /// <summary>
    /// Коллекция мапперов claims для внешних провайдеров
    /// </summary>
    private readonly IEnumerable<IExternalClaimsMapper> _mappers;
    
    /// <summary>
    /// Контроллер для обработки колбэков внешней аутентификации
    /// </summary>
    /// <param name="mappers">Коллекция мапперов claims для внешних провайдеров</param>
    public CallbackController(IEnumerable<IExternalClaimsMapper> mappers)
    {
        _mappers = mappers;
    }

    /// <summary>
    /// Обработка колбэка от внешнего провайдера аутентификации
    /// </summary>
    /// <param name="provider">Название провайдера аутентификации</param>
    /// <returns>Результат аутентификации с перенаправлением</returns>
    /// <exception cref="InvalidOperationException">Когда провайдер не поддерживается</exception>
    /// <exception cref="AuthenticationFailureException">Когда аутентификация не удалась</exception>
    [HttpGet("~/signin-{provider}")]
    public async Task<IActionResult> ExternalLoginCallback(string provider)
    {
        // Получаем маппер claims для указанного провайдера
        var mapper = _mappers.FirstOrDefault(m => m.SupportsProvider(provider));

        // Проверяем поддержку провайдера
        if (mapper is null)
            throw new AuthenticationFailureException($"The provider '{provider}' is not supported. Available Providers: {string.Join(", ", _mappers.Select(m => m.GetType().Name))}");

        // Аутентифицируемся через внешний провайдер
        var result = await HttpContext.AuthenticateAsync(mapper.Provider);

        // Проверяем успешность аутентификации
        if (!result.Succeeded)
            throw new AuthenticationFailureException($"Authentication via the provider '{provider}' failed", result.Failure);

        if (result.Principal == null)
            throw new AuthenticationFailureException($"As a result of authentication via '{provider}', the Principal was not received");

        if (result.Properties == null)
            throw new AuthenticationFailureException($"As a result of authentication via '{provider}', no Properties were received");

        // Маппим claims из внешнего провайдера
        var identity = await mapper.MapAsync(result);

        // Создаем свойства аутентификации для перенаправления
        var properties = new AuthenticationProperties(result.Properties.Items)
        {
            RedirectUri = result.Properties.RedirectUri ?? "/"
        };

        // Выполняем вход
        return SignIn(new ClaimsPrincipal(identity), properties, IdentityConstants.ExternalScheme);
    }
}