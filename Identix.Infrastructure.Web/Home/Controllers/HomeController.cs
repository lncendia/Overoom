using System.Diagnostics;
using System.Net;
using Common.Application.Exceptions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Identix.Application.Abstractions.Exceptions;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Home.ViewModels;

namespace Identix.Infrastructure.Web.Home.Controllers;

/// <summary>
/// Контроллер домашней страницы.
/// </summary>
[AllowAnonymous]
[SecurityHeaders]
public class HomeController : Controller
{
    /// <summary>
    /// Логгер.
    /// </summary>
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Локализатор строк.
    /// </summary>
    private readonly IStringLocalizer<HomeController> _stringLocalizer;

    /// <summary>
    /// Конструктор класса HomeController.
    /// </summary>
    /// <param name="logger">Логгер.</param>
    /// <param name="stringLocalizer">Локализатор строк.</param>
    public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> stringLocalizer)
    {
        _logger = logger;
        _stringLocalizer = stringLocalizer;
    }

    /// <summary>
    /// Действие для отображения домашней страницы.
    /// </summary>
    /// <returns>Результат действия.</returns>
    public IActionResult Index()
    {
        // Возвращаем представление
        return View();
    }

    /// <summary>
    /// Обрабатывает все ошибки приложения, предоставляя единую точку для отображения страницы ошибок
    /// </summary>
    /// <param name="code">HTTP статус код ошибки (опционально)</param>
    /// <returns>Представление с информацией об ошибке</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? code)
    {
        // Обработка исключений
        var exceptionResult = HandleException();
        if (exceptionResult != null)
            return exceptionResult;

        // Обработка ошибок OpenIddict (OAuth/OpenID Connect)
        var oidcResult = HandleOpenIddict();
        if (oidcResult != null)
            return oidcResult;

        // Обработка HTTP кодов (404, 403 и т.д.)
        return HandleStatusCode(code);
    }

    /// <summary>
    /// Обрабатывает исключения, перехваченные глобальным обработчиком ошибок
    /// </summary>
    /// <returns>Представление ошибки для пользовательских исключений, null для системных</returns>
    private ViewResult? HandleException()
    {
        // Получаем информацию об исключении из контекста HTTP
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (context?.Error is not { } ex)
            return null;

        // Логируем ошибку для отладки
        _logger.LogError(ex, "Request processing error");

        // Показываем пользователю только "безопасные" исключения
        if (IsUserFriendlyException(ex))
        {
            return View(new ErrorViewModel
            {
                Message = _stringLocalizer[ex.GetType().Name],
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ReturnUrl = "/"
            });
        }

        // Системные исключения не показываем пользователю
        return null;
    }

    /// <summary>
    /// Обрабатывает ошибки OpenIddict, связанные с OAuth 2.0 и OpenID Connect flows
    /// </summary>
    /// <returns>Представление с описанием ошибки авторизации, null если ошибка не от OpenIddict</returns>
    private ViewResult? HandleOpenIddict()
    {
        // Получаем ответ OpenIddict (ошибки аутентификации/авторизации)
        var response = HttpContext.GetOpenIddictServerResponse();
        if (response is null)
            return null;

        // Показываем пользователю описание ошибки OAuth/OpenID Connect
        return View(new ErrorViewModel
        {
            Message = response.ErrorDescription ?? response.Error!,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ReturnUrl = "/"
        });
    }

    /// <summary>
    /// Обрабатывает стандартные HTTP коды ошибок (404, 500, etc.)
    /// </summary>
    /// <param name="code">HTTP статус код</param>
    /// <returns>Представление с соответствующим сообщением об ошибке</returns>
    private ViewResult HandleStatusCode(int? code)
    {
        // Сообщение по умолчанию для неизвестных ошибок
        var message = _stringLocalizer["DefaultMessage"];

        // Специальное сообщение для 404 ошибки
        if (code == (int)HttpStatusCode.NotFound)
            message = _stringLocalizer["NotFoundMessage"];

        return View(new ErrorViewModel
        {
            Message = message,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ReturnUrl = "/"
        });
    }

    /// <summary>
    /// Определяет, является ли исключение "пользовательским" - безопасным для отображения конечным пользователям
    /// </summary>
    /// <param name="ex">Исключение для проверки</param>
    /// <returns>true - если исключение безопасно для отображения пользователю</returns>
    private static bool IsUserFriendlyException(Exception ex) =>
        // Список исключений, которые можно безопасно показывать пользователю
        // Эти исключения могут быть локализованы без технических деталей
        ex is OpenIdContextException
            or EmailSendException
            or UserNotFoundException
            or LoginAlreadyAssociatedException
            or QueryParameterMissingException
            or LoginNotFoundException
            or InvalidCodeException
            or EmailFormatException
            or UserNameLengthException
            or EmailAlreadyTakenException
            or UserLockoutException
            or TwoFactorAlreadyEnabledException
            or ExternalAuthenticationFailureException;
}