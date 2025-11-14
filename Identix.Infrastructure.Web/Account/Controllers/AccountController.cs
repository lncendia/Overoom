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
        // Проверяем, находимся ли мы в контексте запроса авторизации
        var context = HttpContext.Session.GetOpenIdRequest(returnUrl);

        // создаем вью-модель авторизации
        var model = await BuildLoginViewModelAsync(returnUrl, context);

        // возвращаем view
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
        // Проверяем, находимся ли мы в контексте запроса авторизации
        var context = HttpContext.Session.GetOpenIdRequest(model.ReturnUrl);

        // Устанавливаем в строку запроса закодированную returnUrl, чтоб при изменении локали открылась корректная ссылка (смотреть _Culture.cshtml)
        HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

        // Если модель не валидна
        if (!ModelState.IsValid)
        {
            // Строим заново модель представления
            var loginViewModel = await BuildLoginViewModelAsync(model, context);

            // Возвращаем представление
            return View(loginViewModel);
        }

        try
        {
            // Создает URL-адрес для подтверждения почты
            var callbackUrl = Url.Action("ConfirmEmail", "Registration", null, HttpContext.Request.Scheme)!;

            // Попытка аутентификации пользователя с использованием введенных учетных данных.
            var user = await _mediator.Send(new AuthenticateUserByPasswordCommand
            {
                Email = model.Email!,
                Password = model.Password!,
                ConfirmUrl = callbackUrl,
                ReturnUrl = model.ReturnUrl
            });

            // Устанавливаем пользователю аутентификационные куки
            await _signInManager.SignInAsync(user, model.RememberLogin);

            // Инициализируем событие об успешном входе пользователя
            _logger.LogInformation(
                "User login successful. Email: {Email}, UserId: {UserId}, UserName: {UserName}, ClientId: {ClientId}",
                user.Email, user.Id, user.UserName, context?.ClientId);

            // Перенаправляем по url возврата
            return Redirect(model.ReturnUrl);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                // В случае если исключение ex является UserNotFoundException добавляем код ошибки в модель
                case UserNotFoundException:

                    // Добавляем локализованную ошибку в модель
                    ModelState.AddModelError(string.Empty, _localizer["UserNotFound"]);
                    break;

                // В случае если исключение ex является InvalidPasswordException добавляем код ошибки в модель
                case InvalidPasswordException:

                    // Инициализируем событие об неуспешном входе пользователя
                    _logger.LogInformation(
                        "User login failed (invalid credentials). Email: {Email}, ClientId: {ClientId}",
                        model.Email, context?.ClientId);

                    // Добавляем локализованную ошибку в модель
                    ModelState.AddModelError(string.Empty, _localizer["InvalidCredentials"]);
                    break;

                // В случае если исключение ex является UserLockoutException добавляем код ошибки в модель
                case UserLockoutException:

                    // Инициализируем событие об неуспешном входе пользователя
                    _logger.LogInformation("User login failed (user lockout). Email: {Email}, ClientId: {ClientId}",
                        model.Email, context?.ClientId);

                    // Добавляем локализованную ошибку в модель
                    ModelState.AddModelError(string.Empty, _localizer["UserLockout"]);
                    break;

                // В случае если исключение ex является EmailNotConfirmedException перенаправляем на страницу с письмом
                case EmailNotConfirmedException:
                    return RedirectToAction("ResetPasswordMailSent", new { returnUrl = model.ReturnUrl });

                case TwoFactorRequiredException tfaException:

                    // Получаем, был ли запомнен пользователь системой 2FA
                    var isRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(tfaException.User);

                    // Если пользователь был запомнен
                    if (isRemembered)
                    {
                        // Устанавливаем пользователю аутентификационные куки
                        await _signInManager.SignInAsync(tfaException.User, model.RememberLogin);

                        // Инициализируем событие об успешном входе пользователя
                        _logger.LogInformation(
                            "User login successful (2FA). UserName: {UserName}, UserId: {UserId}, ClientId: {ClientId}",
                            tfaException.User.UserName, tfaException.User.Id, context?.ClientId);

                        // Перенаправляем по url возврата
                        return Redirect(model.ReturnUrl);
                    }

                    // Формируем объект ClaimsIdentity на основе схемы 2FA
                    var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);

                    // Добавляем новый Claim на основе имени пользователя
                    identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, tfaException.User.Id.ToString()));

                    // Осуществляем вход пользователя по схеме 2FA 
                    await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme,
                        new ClaimsPrincipal(identity));

                    // Перенаправляем пользователя на страницу прохождения 2FA
                    return RedirectToAction("LoginTwoStep", "TwoFactor",
                        new { returnUrl = model.ReturnUrl, rememberMe = model.RememberLogin });

                // Если исключение ex не является ни одним их типов, то вызываем исключение дальше
                default: throw;
            }

            // Строим заново модель представления
            var loginViewModel = await BuildLoginViewModelAsync(model, context);

            // Возвращаем представление
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
        // Возвращаем представление с моделью
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
        // Если модель не валида - возвращаем представление
        if (!ModelState.IsValid) return View(model);

        // Устанавливаем в строку запроса закодированную returnUrl, чтоб при изменении локали открылась корректная ссылка (смотреть _Culture.cshtml)
        HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

        // Генерируем url для смены пароля
        var url = Url.Action(
            "NewPassword", "Account", new { returnUrl = model.ReturnUrl }, HttpContext.Request.Scheme)!;

        try
        {
            // Отправляем команду на смену пароля
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

        // Перенаправляем на страницу MailSent
        return RedirectToAction("ResetPasswordMailSent", new { returnUrl = model.ReturnUrl });
    }

    /// <summary>
    /// Возвращает представление для страницы "MailSent".
    /// </summary>
    /// <returns>Результат действия для страницы "MailSent".</returns>
    public IActionResult ConfirmEmailMailSent(string returnUrl = "/") =>
        View("MailSent", new MailSentViewModel(_localizer.GetString("MailSent_ConfirmEmail"), returnUrl));

    /// <summary>
    /// Возвращает представление для страницы "ResetPasswordMailSent".
    /// </summary>
    /// <returns>Результат действия для страницы "MailSent".</returns>
    public IActionResult ResetPasswordMailSent(string returnUrl = "/") =>
        View("MailSent", new MailSentViewModel(_localizer.GetString("MailSent_ResetPassword"), returnUrl));

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
        // Выбрасывание исключения QueryParameterMissingException, если параметр id отсутствует
        if (!id.HasValue) throw new QueryParameterMissingException(nameof(id));

        // Выбрасывание исключения QueryParameterMissingException, если параметр code отсутствует
        if (string.IsNullOrEmpty(code)) throw new QueryParameterMissingException(nameof(code));

        // Возвращаем представление с моделью
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
        // Устанавливаем в строку запроса закодированную returnUrl, чтоб при изменении локали открылась корректная ссылка (смотреть _Culture.cshtml)
        HttpContext.Request.QueryString =
            new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl) + "&Id=" +
                            HttpUtility.UrlEncode(model.UserId.ToString()) + "&Code=" +
                            HttpUtility.UrlEncode(model.Code));

        // Если модель не валида - возвращаем представление
        if (!ModelState.IsValid) return View(model);

        try
        {
            // Отправляем команду на сброс пароля и установку нового пароля
            await _mediator.Send(new RecoverPasswordCommand
            {
                Code = model.Code!,
                UserId = model.UserId,
                NewPassword = model.NewPassword!
            });

            // Перенаправляем пользователя на страницу входа и устанавливаем returnUrl
            return RedirectToAction("Login", new { returnUrl = model.ReturnUrl });
        }
        catch (PasswordValidationException ex)
        {
            // Добавляем код(ключ) всех ошибок, содержащихся в ValidationErrors
            foreach (var error in ex.ValidationErrors)
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
        // Проверяем, находимся ли мы в контексте запроса авторизации
        var context = HttpContext.Session.GetOpenIdRequest(returnUrl);

        if (context == null)
            return Redirect(returnUrl);

        HttpContext.Session.DenyConsent(context, User.GetId());

        // Перенаправляем по url возврата
        return Redirect(returnUrl);
    }

    /// <summary>
    /// Показать страницу выхода
    /// </summary>
    [HttpGet]
    public IActionResult Logout(string returnUrl = "/")
    {
        // Строем модель для представления
        var inputModel = new LogoutInputModel { ReturnUrl = returnUrl };

        // Показываем страницу подтверждения выхода
        return View(inputModel);
    }

    /// <summary>
    /// Обработка постбэка страницы выхода
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutInputModel model)
    {
        // Если пользователь не аутентифицирован
        if (User.Identity?.IsAuthenticated != true)
        {
            // Сразу перенаправляем назад
            return Redirect(model.ReturnUrl);
        }

        // Отчищаем cookie аутентификации
        await _signInManager.SignOutAsync();

        // Вызываем событие выхода из системы
        _logger.LogInformation("User logout successful. Id: {Id}", User.Id());

        // Сразу перенаправляем назад
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
        // Получаем все внешние провайдеры идентификации (oauth схемы)
        var schemes = (await _signInManager.GetExternalAuthenticationSchemesAsync()).Select(s => s.Name);

        // Устанавливаем, что по умолчанию включен локальный провайдер (вход по логину и паролю)
        var enableLocalIdentityProvider = true;

        // Если контекст аутентификации IdentityServer null
        if (context == null)
        {
            // Формируем вью-модель входа в систему
            return new LoginViewModel
            {
                // Url возврата
                ReturnUrl = returnUrl,

                // Флаг, включен ли локальный провайдер идентификации
                EnableLocalLogin = enableLocalIdentityProvider,

                // Массив доступных внешних провайдеров
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
        return new LoginViewModel
        {
            // Url возврата
            ReturnUrl = returnUrl,

            // Флаг, включен ли локальный провайдер идентификации
            EnableLocalLogin = enableLocalIdentityProvider,

            // Массив доступных внешних провайдеров
            ExternalProviders = schemes.ToArray(),

            // Подсказка для логина
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
        // формируем вью модель из returnUrl
        var vm = await BuildLoginViewModelAsync(model.ReturnUrl, context);

        // добавляем email из прилетевшей в контроллер модели
        vm.Email = model.Email;

        // добавляем rememberLogin из прилетевшей в контроллер модели
        vm.RememberLogin = model.RememberLogin;

        // добавляем password из прилетевшей в контроллер модели
        vm.Password = model.Password;

        // Возвращаем модель
        return vm;
    }
}