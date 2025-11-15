using System.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions;
using Identix.Application.Abstractions.Commands.Create;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Extensions;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Registration.InputModels;
using Identix.Infrastructure.Web.Registration.ViewModels;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.Registration.Controllers;

/// <summary>
/// Контроллер для прохождения регистрации.
/// </summary>
[SecurityHeaders]
public class RegistrationController : Controller
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
    private readonly IStringLocalizer<RegistrationController> _localizer;

    /// <summary>
    /// Логгер
    /// </summary>
    private readonly ILogger<RegistrationController> _logger;

    /// <summary>
    /// Конструктор контроллера для прохождения регистрации.
    /// </summary>
    /// <param name="signInManager">Предоставляет API для входа пользователя</param>
    /// <param name="localizer">Локализатор</param>
    /// <param name="mediator">Медиатор</param>
    /// <param name="logger">Логгер</param>
    public RegistrationController(SignInManager<AppUser> signInManager,
        IStringLocalizer<RegistrationController> localizer, ISender mediator, ILogger<RegistrationController> logger)
    {
        _signInManager = signInManager;
        _localizer = localizer;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Метод отдает View регистрации
    /// </summary>
    /// <param name="returnUrl">Url для возврата</param>
    [HttpGet]
    public async Task<IActionResult> Registration(string returnUrl = "/")
    {
        // Проверяем, находимся ли мы в контексте запроса авторизации
        var context = HttpContext.Session.GetOpenIdRequest(returnUrl);

        // создаем вью-модель регистрации
        var vm = await BuildRegisterViewModelAsync(returnUrl, context);

        // возвращаем view
        return View(vm);
    }

    /// <summary>
    /// Обработка регистрации пользователя
    /// </summary>
    /// <param name="model">Модель входа в систему</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Registration(RegistrationInputModel model)
    {
        // Проверяем, находимся ли мы в контексте запроса авторизации
        var context = HttpContext.Session.GetOpenIdRequest(model.ReturnUrl);

        // Устанавливаем в строку запроса закодированную returnUrl, чтоб при изменении локали открылась корректная ссылка (смотреть _Culture.cshtml)
        HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

        // Если данные не валидны
        if (!ModelState.IsValid)
        {
            var vm = await BuildRegisterViewModelAsync(model, context);
            return View(vm);
        }

        // Создает URL-адрес для подтверждения почты
        var callbackUrl = Url.Action("ConfirmEmail", "Registration", null, HttpContext.Request.Scheme)!;

        // Получаем сервис IRequestCultureFeature
        var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();

        // Получаем текущую локаль
        var locale = requestCulture!.RequestCulture.UICulture.Name.GetLocalization();

        try
        {
            // Отправляем команду на создание пользователя
            var user = await _mediator.Send(new CreateUserCommand
            {
                Email = model.Email!,
                Password = model.Password!,
                ConfirmUrl = callbackUrl,
                Locale = locale,
                ReturnUrl = model.ReturnUrl
            });

            // Инициализируем событие об успешной регистрации пользователя
            _logger.LogInformation(
                "User registration successful. Email: {Email}, UserId: {UserId}, UserName: {UserName}, ClientId: {ClientId}",
                user.Email, user.Id, user.UserName, context?.ClientId);

            return RedirectToAction("ConfirmEmailMailSent", "Account", new { returnUrl = model.ReturnUrl });
        }
        catch (Exception ex)
        {
            // Проверяем какое исключение мы словили и добавляем в ModelState соответсвующее значение.
            switch (ex)
            {
                // В случае если исключение ex является EmailAlreadyTakenException добавляем код ошибки в модель
                case EmailAlreadyTakenException:
                    ModelState.AddModelError("", _localizer["UserAlreadyExist"]);
                    break;

                // В случае если исключение ex является EmailFormatException добавляем код ошибки в модель
                case EmailFormatException:
                    ModelState.AddModelError("", _localizer["EmailFormatInvalid"]);
                    break;

                // В случае если исключение ex является PasswordValidationException
                // Добавляеем код(ключ) всех ошибок, содержащихся в passwordValidationException.ValidationErrors
                case PasswordValidationException passwordValidationException:
                    foreach (var error in passwordValidationException.ValidationErrors)
                    {
                        ModelState.AddModelError("", _localizer[error.Key]);
                    }

                    break;

                // Если исключение ex не является ни одним их типов, то вызываем исключение дальше
                default: throw;
            }

            // Создаем модель представления регистрации
            var vm = await BuildRegisterViewModelAsync(model, context);

            // Возвращаем представление
            return View(vm);
        }
    }

    /// <summary>
    /// Метод подтверждения email
    /// </summary>
    /// <param name="id">Id пользователя</param>
    /// <param name="code">Токен для подтверждения email пользователя</param>
    /// <param name="returnUrl">Url для возврата</param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(Guid? id, string? code, string returnUrl = "/")
    {
        // проверяем входящие данные
        if (!id.HasValue) throw new QueryParameterMissingException(nameof(id));

        // проверяем входящие данные
        if (code == null) throw new QueryParameterMissingException(nameof(code));

        // Верифицируем email
        await _mediator.Send(new VerifyEmailCommand
        {
            UserId = id.Value,
            Code = code
        });

        // Возвращаем представление
        return View(new ConfirmEmailViewModel(returnUrl));
    }

    /// <summary>
    /// Создает модель представления регистрации
    /// </summary>
    /// <param name="returnUrl">Url для возврата</param>
    /// <param name="context">Контекст авторизации</param>
    /// <returns>Вью-модель регистрации в систему</returns>
    private async Task<RegistrationViewModel> BuildRegisterViewModelAsync(string returnUrl, OpenIddictRequest? context)
    {
        // Получаем все внешние провайдеры идентификации (oauth схемы)
        var schemes = (await _signInManager.GetExternalAuthenticationSchemesAsync()).Select(s => s.Name);

        // Устанавливаем, что по умолчанию включен локальный провайдер (вход по логину и паролю)
        var enableLocalIdentityProvider = true;

        // Если контекст аутентификации IdentityServer не null
        if (context == null)
        {
            // Формируем вью-модель входа в систему
            return new RegistrationViewModel
            {
                ReturnUrl = returnUrl,
                EnableLocalLogin = enableLocalIdentityProvider,
                ExternalProviders = schemes.ToArray()
            };
        }

        // Если в контексте аутентификации запрошен какой-то конкретный провайдер
        if (context.IdentityProvider != null)
        {
            // Если это локальный провайдер (вход с помощью логина и пароля)
            if (context.IdentityProvider == Constants.IdentityProviders.Local)
            {
                // Удаляем все внешние провайдеры (так как запрошен локальный)
                schemes = [];
            }
            else
            {
                // Отключаем локальный провайдер, так как запрошен внешний
                enableLocalIdentityProvider = false;

                // Удаляем все провайдеры кроме запрашиваемого
                schemes = schemes.Where(provider => provider == context.IdentityProvider);
            }
        }

        // формируем вью-модель входа в систему
        return new RegistrationViewModel
        {
            ReturnUrl = returnUrl,
            EnableLocalLogin = enableLocalIdentityProvider,
            ExternalProviders = schemes.ToArray(),
            Email = context.LoginHint
        };
    }

    /// <summary>
    /// Построить асинхронную модель представления входа
    /// </summary>
    /// <param name="model">Модель входа в систему</param>
    /// <param name="context">Контекст авторизации</param>
    /// <returns>Вью-модель входа в систему</returns>
    private async Task<RegistrationViewModel> BuildRegisterViewModelAsync(RegistrationInputModel model,
        OpenIddictRequest? context)
    {
        // Построить асинхронную модель представления входа
        var vm = await BuildRegisterViewModelAsync(model.ReturnUrl, context);

        // устанавливаем прилетевшую в контроллер почту
        vm.Email = model.Email;

        // устанавливаем прилетевший в контроллер пароль
        vm.Password = model.Password;

        // устанавливаем прилетевший в контроллер пароль
        vm.PasswordConfirm = model.PasswordConfirm;

        // возвращаем вью-модель
        return vm;
    }
}