using System.Security.Claims;
using System.Web;
using AuthService.Application.Abstractions.Commands.Authentication;
using AuthService.Application.Abstractions.Commands.TwoFactor;
using AuthService.Application.Abstractions.Entities;
using AuthService.Application.Abstractions.Enums;
using AuthService.Application.Abstractions.Exceptions;
using AuthService.Application.Abstractions.Queries;
using AuthService.Infrastructure.Web.TwoFactor.InputModels;
using AuthService.Infrastructure.Web.TwoFactor.ViewModels;
using IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AuthService.Infrastructure.Web.TwoFactor.Controllers;

/// <summary>
/// Контроллер для изменения настроек аккаунта
/// </summary>
/// <param name="mediator">Медиатор</param>
/// <param name="signInManager">Предоставляет API для входа пользователя.</param>
/// <param name="localizer">Локализатор</param>
public class TwoFactorController(
    IMediator mediator,
    SignInManager<UserData> signInManager,
    IStringLocalizer<TwoFactorController> localizer)
    : Controller
{
    /// <summary>
    /// Точка входа на страницу подключения 2FA
    /// </summary>
    /// <param name="returnUrl">адрес Url переадресации</param>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Setup(string returnUrl = "/")
    {
        // Создаем вью-модель подключения 2FA
        var model = await BuildSetupViewModelAsync(returnUrl);

        // возвращаем view
        return View(model);
    }

    /// <summary>
    /// Обработка подключения 2FA
    /// </summary>
    /// <param name="model">Модель подключения 2FA</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> VerifySetup(SetupTwoFactorInputModel model)
    {
        // Устанавливаем в строку запроса закодированную returnUrl, чтоб при изменении локали открылась корректная ссылка (смотреть _Culture.cshtml)
        HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

        // Если модель не валидна
        if (!ModelState.IsValid)
        {
            // строим заного модель представления
            var viewModel = await BuildSetupViewModelAsync(model.ReturnUrl);

            // передаем в модель введенный ранее код
            viewModel.Code = model.Code;

            // возвращаем представление
            return View("Setup", viewModel);
        }

        try
        {
            // попытка подключения 2FA
            var codes = await mediator.Send(new VerifySetupTwoFactorTokenCommand
            {
                UserId = User.Id(),
                Code = model.Code!
            });

            // перенаправляем по url возврата
            return View("VerifySetup", new RecoveryCodesViewModel
            {
                RecoveryCodes = codes,
                ReturnUrl = model.ReturnUrl
            });
        }
        catch (InvalidCodeException)
        {
            // Добавляем локализованную ошибку в модель
            ModelState.AddModelError(string.Empty, localizer["InvalidCode"]);

            // строим заного модель представления
            var viewModel = await BuildSetupViewModelAsync(model.ReturnUrl);

            // передаем в модель введенный ранее код
            viewModel.Code = model.Code;

            // возвращаем представление
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
        // Возвращаем представление пользователю
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
        // Устанавливаем в строку запроса закодированную returnUrl, чтоб при изменении локали открылась корректная ссылка (смотреть _Culture.cshtml)
        HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

        // Если модель не валидна
        if (!ModelState.IsValid)
        {
            // Очищаем список ошибок модели
            ModelState.Clear();

            // Заного формируем представление и возвращаем пользователю
            return View(await BuildLoginTwoStepViewModelAsync(model.RememberMe, model.ReturnUrl, model.CodeType));
        }

        // Получаем провайдер из контекста запроса
        var loginProvider = User.FindFirstValue(JwtClaimTypes.IdentityProvider);

        try
        {
            // Отправляем команду на прохождение 2FA
            var user = await mediator.Send(new AuthenticateTwoFactorCommand
            {
                Code = model.Code!,
                UserId = User.Id(),
                Type = model.CodeType
            });

            // Аутентифицируем пользователя
            await signInManager.SignInAsync(user, model.RememberMe, loginProvider);

            // Удаляем куки с идентификатором пользователя
            await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

            // если стоит флаг о запоминании пользователя
            if (model.RememberMe)
            {
                // устанавливаем куки
                await signInManager.RememberTwoFactorClientAsync(user);
            }

            // Перенаправляем на адрес возврата
            return Redirect(model.ReturnUrl);
        }
        catch (InvalidCodeException)
        {
            ModelState.Clear();

            // // Инициализируем событие об неуспешном входе пользователя
            // await _events.RaiseAsync(new UserLoginFailureEvent(model.Email,
            //     "Invalid credentials", clientId: context?.Client.ClientId));


            // Добавляем локализованную ошибку в модель
            ModelState.AddModelError(string.Empty, localizer["InvalidCode"]);

            // Заного формируем представление и возвращаем пользователю
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
            // Отправляем команду на отправку письма с кодом пользователю
            await mediator.Send(new RequestTwoFactorCodeEmailCommand { UserId = User.Id() });
        }
        catch
        {
            // Возвращаем пустой ответ с кодом 400
            return BadRequest();
        }

        // Возвращаем пустой ответ с кодом 200
        return Ok();
    }


    /// <summary>
    /// Точка входа на сброс 2FA
    /// </summary>
    /// <param name="returnUrl">Адрес url возврата</param>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ResetTwoFactor(string returnUrl = "/")
    {
        // Возвращаем представление сброса 2FA
        return View(await BuildResetTwoFactorViewModelAsync(returnUrl, CodeType.Authenticator));
    }

    /// <summary>
    /// Обработка сброса 2FA
    /// </summary>
    /// <param name="model">Модель сброса 2FA</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ResetTwoFactor(TwoFactorAuthenticateInputModel model)
    {
        // Если модель не валидна
        if (!ModelState.IsValid)
        {
            // Очищаем список ошибок модели
            ModelState.Clear();

            // Заного формируем представление и возвращаем пользователю
            return View(await BuildResetTwoFactorViewModelAsync(model.ReturnUrl, model.CodeType));
        }

        try
        {
            // Отправляем команду на сброс 2FA
            var user = await mediator.Send(new ResetTwoFactorCommand
            {
                UserId = User.Id(),
                Code = model.Code!,
                Type = model.CodeType
            });

            // обновляем вход пользователя, чтобы он остался аутентифицирован после сброса аутентификатора
            await signInManager.RefreshSignInAsync(user);

            // Перенаправляем пользователя на адрес возврата
            return Redirect(model.ReturnUrl);
        }
        catch (InvalidCodeException)
        {
            // Добавляем локализованную ошибку в модель
            ModelState.AddModelError(string.Empty, localizer["InvalidCode"]);

            // Заного формируем представление и возвращаем пользователю
            return View(await BuildResetTwoFactorViewModelAsync(model.ReturnUrl, model.CodeType));
        }
    }


    /*****************************************/
    /* вспомогательные API для AccountController */
    /*****************************************/

    /// <summary>
    /// Метод формирует модель представления для подключения 2FA
    /// </summary>
    /// <param name="returnUrl">Url возврата</param>
    /// <returns>Модель представления подключения 2FA</returns>
    private async Task<SetupTwoFactorViewModel> BuildSetupViewModelAsync(string returnUrl)
    {
        // Отправляем команду на получение аутентификатора и добавления его пользователю
        var result = await mediator.Send(new SetupTwoFactorCommand { UserId = User.Id() });

        // обновляем вход пользователя, чтобы он остался аутентифицирован после получения аутентификатора
        await signInManager.RefreshSignInAsync(result.user);

        // возвращаем представление для подключения 2FA
        return new SetupTwoFactorViewModel(result.token, result.Item1.UserName!, "PJMS")
            { ReturnUrl = returnUrl };
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
        // Отправляем команду на получение аутентификатора и добавления его пользователю
        var user = await mediator.Send(new UserByIdQuery { UserId = User.Id() });

        // возвращаем представление для подключения 2FA
        return new LoginTwoStepViewModel
        {
            CodeType = codeType,
            NeedShowEmail = user.EmailConfirmed,
            RememberMe = rememberMe,
            ReturnUrl = returnUrl
        };
    }

    /// <summary>
    /// Метод формирует модель представления для сброса 2FA
    /// </summary>
    /// <param name="returnUrl">Url возврата</param>
    /// <param name="codeType">Тип генерации кода</param>
    /// <returns></returns>
    private async Task<ResetTwoFactorViewModel> BuildResetTwoFactorViewModelAsync(string returnUrl, CodeType codeType)
    {
        // Отправляем команду на получение аутентификатора и добавления его пользователю
        var user = await mediator.Send(new UserByIdQuery { UserId = User.Id() });

        // возвращаем представление для сброса 2FA
        return new ResetTwoFactorViewModel
        {
            CodeType = codeType,
            ReturnUrl = returnUrl,
            NeedShowEmail = user.EmailConfirmed
        };
    }
}