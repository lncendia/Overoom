using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Commands.External;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Queries;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Settings.InputModels;
using Identix.Infrastructure.Web.Settings.ViewModels;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.Settings.Controllers;

/// <summary>
/// Контроллер для изменения настроек аккаунта
/// </summary>
[Authorize]
[SecurityHeaders]
public class SettingsController : Controller
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
    private readonly IStringLocalizer<SettingsController> _localizer;

    /// <summary>
    /// Конструктор контроллера для прохождения аутентификации.
    /// </summary>
    /// <param name="mediator">Медиатор</param>
    /// <param name="signInManager">Предоставляет API для входа пользователя.</param>
    /// <param name="localizer">Локализатор</param>
    public SettingsController(ISender mediator, SignInManager<AppUser> signInManager,
        IStringLocalizer<SettingsController> localizer)
    {
        _mediator = mediator;
        _signInManager = signInManager;
        _localizer = localizer;
    }

    /// <summary>
    /// Страница контроллера по умолчанию
    /// </summary>
    /// <param name="model">Модель данных, необходимых для отображения страницы</param>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] SettingsInputModel model)
    {
        // Получаем аутентифицированного пользователя
        var user = await _mediator.Send(new UserByIdQuery { Id = User.Id() });

        // Добавляем сообщение об ошибке, если оно есть
        if (!string.IsNullOrEmpty(model.ErrorMessage)) ModelState.AddModelError("", model.ErrorMessage);

        // Создаем модель представления
        var settingsModel = await BuildViewModelAsync(user, model);

        // Возвращаем представление с моделью настроек
        return View(settingsModel);
    }

    /// <summary>
    /// Метод, который вызывается при перенаправлении на внешний провайдер аутентификации для вызова вызова аутентификации.
    /// </summary>
    /// <param name="provider">Имя внешнего провайдера аутентификации</param>
    /// <param name="returnUrl">Url для возврата</param>
    /// <returns>Результат вызова аутентификации</returns>
    [HttpGet]
    public IActionResult Challenge(string? provider, string returnUrl = "/")
    {
        // Проверяем, что имя провайдера не пустое или null
        if (string.IsNullOrEmpty(provider)) throw new QueryParameterMissingException(nameof(provider));

        // Создаем URL для обратного вызова после аутентификации
        var redirectUrl = Url.Action("ExternalLoginCallback", "Settings", new { ReturnUrl = returnUrl });

        // Конфигурируем свойства аутентификации для внешнего провайдера
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        // Возвращаем вызов аутентификации с указанным провайдером и свойствами
        return new ChallengeResult(provider, properties);
    }

    /// <summary>
    /// Обрабатывает обратный вызов внешней аутентификации.
    /// </summary>
    /// <param name="returnUrl">Url для возврата</param>
    /// <returns>Результат действия IActionResult.</returns>
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
    {
        // Получаем информацию о внешней аутентификации.
        var info = await _signInManager.GetExternalLoginInfoAsync();

        // Если информация о внешнем провайдере недоступна, прерываем процесс аутентификации
        if (info == null)
            throw new ExternalAuthenticationFailureException(
                "Couldn't get information about an external authentication");

        // Отправляем команду на добавление внешнего входа и получаем пользователя с обновленными данными
        var user = await _mediator.Send(new AddUserExternalLoginCommand { UserId = User.Id(), LoginInfo = info });

        // Отчищаем куки данных от внешнего провайдера
        await HttpContext.SignOutAsync(info.AuthenticationProperties);

        // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
        await _signInManager.RefreshSignInAsync(user);

        // Перенаправляем пользователя на указанный URL
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = 1,
            ReturnUrl = returnUrl,
            Message = string.Format(_localizer["ProviderLinked"], info.ProviderDisplayName)
        });
    }

    /// <summary>
    /// Метод, который удаляет вход внешнего провайдера аутентификации у пользователя.
    /// </summary>
    /// <param name="model">Модель с данными для удаления провайдера</param>
    /// <returns>Результат удаления входа</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogin(RemoveLoginInputModel model)
    {
        // Объявление переменной message и errorMessage типа string
        string? message = null, errorMessage = null;
        
        // Присвоение описания первой ошибки валидации переменной message, если модель не валидна
        if (!ModelState.IsValid) errorMessage = GetFirstError();
        else
        {
            // Отправляем команду на удаление внешнего логина и получаем пользователя с обновленными данными
            var user = await _mediator.Send(
                new RemoveUserExternalLoginCommand { UserId = User.Id(), Provider = model.Provider! });

            // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
            await _signInManager.RefreshSignInAsync(user);

            // Получаем отображаемое имя провайдера
            var providerDisplayName = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                                      .FirstOrDefault(p => p.Name == model.Provider)?.DisplayName
                                      ?? model.Provider;

            message = string.Format(_localizer["ProviderUnlinked"], providerDisplayName);
        }

        // Перенаправляем пользователя на указанный URL
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = 1,
            ReturnUrl = model.ReturnUrl,
            Message = message,
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// Метод, который заканчивает другие сессии у пользователя
    /// </summary>
    /// <param name="model">Модель с данными для закрытия сессий</param>
    /// <returns>Результат закрытия сессий</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseOtherSessions(CloseSessionsInputModel model)
    {
        // Отправляем команду на закрытие всех других сессий, возвращаем данного пользователя
        var user = await _mediator.Send(new UpdateSecurityStampCommand { UserId = User.Id() });

        // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
        await _signInManager.RefreshSignInAsync(user);

        // Перенаправляем на действие "Index" с указанными параметрами returnUrl, expandElem и message
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = model.ExpandElement,
            ReturnUrl = model.ReturnUrl,
            Message = _localizer["SessionsClosed"]
        });
    }

    /// <summary>
    /// Метод, который изменяет пароль пользователя.
    /// </summary>
    /// <param name="model">Модель с данными для смены пароля</param>
    /// <returns>Результат смены пароля</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordInputModel model)
    {
        // Объявление переменной message и errorMessage типа string
        string? message = null, errorMessage = null;
        
        // Присвоение описания первой ошибки валидации переменной message, если модель не валидна
        if (!ModelState.IsValid) errorMessage = GetFirstError();
        else
        {
            try
            {
                // Отправляем команду на смену пароля и получаем пользователя с обновленными данными
                var user = await _mediator.Send(new ChangePasswordCommand(model.OldPassword, model.NewPassword!)
                {
                    UserId = User.Id()
                });

                // Устанавливаем сообщение "PasswordChanged"
                message = _localizer["PasswordChanged"].ToString();

                // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
                await _signInManager.RefreshSignInAsync(user);
            }
            catch (Exception ex)
            {
                // Проверяем какое исключение мы словили и устанавливаем соответствующее значение в message
                switch (ex)
                {
                    // В случае если исключение ex является OldPasswordNeededException устанавливаем соответствующее сообщение
                    case PasswordNeededException:
                        errorMessage = _localizer["OldPasswordNeeded"];
                        break;

                    // В случае если исключение ex является PasswordValidationException устанавливаем соответствующее сообщение
                    case PasswordValidationException passwordValidationException:

                        // Формируем перечисление из локализованных ошибок валидации пароля
                        var errorsEnumerable = passwordValidationException.ValidationErrors
                            .Select(code => _localizer[code.Key]);

                        // Формируем строку из перечисления ошибок через запятую
                        errorMessage = string.Join(", ", errorsEnumerable);
                        break;

                    // В случае если исключение ex является ArgumentException устанавливаем соответствующее сообщение
                    case ArgumentException:

                        // Если старый пароль совпадает с новым, то устанавливаем соответствующее сообщение
                        errorMessage = _localizer["OldPasswordMatchNew"];
                        break;

                    // Если исключение ex не является ни одним их типов, то вызываем исключение дальше
                    default: throw;
                }
            }
        }

        // Перенаправляем на действие "Index" с указанными параметрами returnUrl, expandElem и message
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = 2,
            ReturnUrl = model.ReturnUrl,
            Message = message,
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// Метод для запроса изменения адреса электронной почты пользователя.
    /// </summary>
    /// <param name="model">Модель ввода, содержащая новый адрес электронной почты и другие соответствующие данные.</param>
    /// <returns>Объект IActionResult, представляющий результат операции.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestChangeEmail(RequestChangeEmailInputModel model)
    {
        // Объявление переменной message и errorMessage типа string
        string? message = null, errorMessage = null;

        // Присвоение описания первой ошибки валидации переменной message, если модель не валидна
        if (!ModelState.IsValid)
        {
            errorMessage = GetFirstError();
        }
        else
        {
            // Формирование URL-адреса обратного вызова для изменения адреса электронной почты
            var resetUrl = Url.Action("ChangeEmail", "Settings", null, HttpContext.Request.Scheme)!;

            try
            {
                await _mediator.Send(new RequestChangeEmailCommand
                {
                    UserId = User.Id(),
                    NewEmail = model.Email!,
                    Password = model.Password,
                    ResetUrl = resetUrl,
                    ReturnUrl = model.ReturnUrl
                });

                // Присвоение локализованной строки "EmailChangeRequested" переменной message
                message = _localizer["EmailChangeRequested"];
            }
            catch (PasswordNeededException)
            {
                errorMessage = _localizer["PasswordNeeded"];
            }
            catch (InvalidPasswordException)
            {
                errorMessage = _localizer["InvalidPassword"];
            }
        }

        // Перенаправление на действие "Index" с указанными данных
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = 3,
            Message = message,
            ReturnUrl = model.ReturnUrl,
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// Метод для изменения адреса электронной почты пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="email">Новый адрес электронной почты.</param>
    /// <param name="code">Код подтверждения изменения адреса электронной почты.</param>
    /// <param name="returnUrl">Url для возврата</param>
    /// <returns>Объект IActionResult, представляющий результат операции.</returns>
    [HttpGet]
    public async Task<IActionResult> ChangeEmail(Guid? id, string? email, string? code, string returnUrl = "/")
    {
        // Выбрасывание исключения QueryParameterMissingException, если параметр id отсутствует
        if (!id.HasValue || id.Value != User.Id()) throw new QueryParameterMissingException(nameof(id));

        // Выбрасывание исключения QueryParameterMissingException, если параметр email отсутствует
        if (string.IsNullOrEmpty(email)) throw new QueryParameterMissingException(nameof(email));

        // Выбрасывание исключения QueryParameterMissingException, если параметр code отсутствует
        if (string.IsNullOrEmpty(code)) throw new QueryParameterMissingException(nameof(code));

        // Объявление переменной message и errorMessage типа string
        string? message = null, errorMessage = null;

        try
        {
            // Отправляем команду на смену почты и получаем пользователя с обновленными данными
            var user = await _mediator.Send(new ChangeEmailCommand
            {
                Code = code,
                NewEmail = email,
                UserId = id.Value
            });

            // Устанавливаем сообщение о том, что почта изменена
            message = _localizer["EmailChanged"];

            // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
            await _signInManager.RefreshSignInAsync(user);
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                // В случае если исключение ex является EmailAlreadyTakenException устанавливаем соответствующее сообщение
                case EmailAlreadyTakenException:
                    errorMessage = _localizer["EmailAlreadyTaken"];
                    break;

                // В случае если исключение ex является EmailFormatException устанавливаем соответствующее сообщение
                case EmailFormatException:
                    errorMessage = _localizer["EmailFormatInvalid"];
                    break;

                // Если исключение ex не является ни одним их типов, то вызываем исключение дальше
                default: throw;
            }
        }

        // Перенаправление на действие "Index" с указанными данных
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = 3,
            ReturnUrl = returnUrl,
            Message = message,
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// Метод для изменения имени пользователя.
    /// </summary>
    /// <param name="model">Модель с данными смены имени.</param>
    /// <returns>Объект IActionResult, представляющий результат операции.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeName(ChangeNameInputModel model)
    {
        // Объявление переменной message и errorMessage типа string
        string? message = null, errorMessage = null;
        
        // Присвоение описания первой ошибки валидации переменной message, если модель не валидна
        if (!ModelState.IsValid) errorMessage = GetFirstError();

        // Иначе
        else
        {
            try
            {
                var user = await _mediator.Send(new ChangeNameCommand
                {
                    UserId = User.Id(),
                    Name = model.Username!
                });

                // Устанавливаем сообщение о том, что имя изменено
                message = _localizer["UserNameChanged"];

                // Так как Security Stamp у пользователя обновился, то переавторизуем его, чтобы обновить куки
                await _signInManager.RefreshSignInAsync(user);
            }
            catch (UserNameLengthException)
            {
                errorMessage = _localizer["UserNameLengthInvalid"];
            }
        }

        // Перенаправление на действие "Index" с указанными данных
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = 4,
            ReturnUrl = model.ReturnUrl,
            Message = message,
            ErrorMessage = errorMessage
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ChangeAvatar(ChangeAvatarInputModel model)
    {
        // Объявление переменной message и errorMessage типа string
        string? message = null, errorMessage = null;

        // Присвоение описания первой ошибки валидации переменной message, если модель не валидна
        if (!ModelState.IsValid) errorMessage = GetFirstError();

        // Иначе если размер аватара превышает 15 Мб
        else if (model.File!.Length > 15728640) errorMessage = _localizer["WrongFileSize"];

        // Иначе
        else
        {
            // Открываем поток с файлом модели
            await using var stream = model.File!.OpenReadStream();

            // Отправляем команду на смену аватара
            await _mediator.Send(new ChangeAvatarCommand
            {
                // Идентификатор пользователя
                UserId = User.Id(),

                // Поток с аватаром
                Thumbnail = stream
            });

            // Устанавливаем сообщение о том, что почта изменена
            message = _localizer["AvatarChanged"];
        }

        // Перенаправление на действие "Index" с указанными данных
        return RedirectToAction("Index", new SettingsInputModel
        {
            ExpandElement = 5,
            ReturnUrl = model.ReturnUrl,
            Message = message,
            ErrorMessage = errorMessage
        });
    }

    /// <summary>
    /// Метод, отвечающий за построение модели представления для страницы настроек.
    /// </summary>
    /// <param name="user">Объект пользователя</param>
    /// <param name="model">Модель данных, необходимых для отображения страницы</param>
    /// <returns>Модель представления настроек</returns>
    private async Task<SettingsViewModel> BuildViewModelAsync(AppUser user, SettingsInputModel model)
    {
        // Получаем список входов пользователя
        var logins = await _mediator.Send(new UserLoginsQuery { Id = user.Id });

        // Получаем все внешние провайдеры идентификации (oauth схемы)
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

        // Создаем список для хранения внешних провайдеров аутентификации
        var userSchemes = new List<ExternalProvider>();

        // Перебираем все схемы аутентификации для определения доступных провайдеров
        foreach (var authenticationScheme in schemes)
        {
            // Проверяем, связан ли провайдер с пользователем
            var isAssociated = logins.Any(login => login == authenticationScheme.Name);

            // Создаем объект ExternalProvider и добавляем его в список
            userSchemes.Add(new ExternalProvider
            {
                DisplayName = authenticationScheme.DisplayName ?? authenticationScheme.Name,
                AuthenticationScheme = authenticationScheme.Name,
                IsAssociated = isAssociated
            });
        }

        // Создаем модель представления настроек с переданными внешними провайдерами и returnUrl
        var settingsModel = new SettingsViewModel
        {
            ReturnUrl = model.ReturnUrl,
            ExternalProviders = userSchemes,
            HasPassword = user.PasswordHash != null,
            ExpandElement = model.ExpandElement,
            Email = user.Email!,
            Message = model.Message,
            TwoFactorEnabled = user.TwoFactorEnabled,
            UserName = user.UserName!,
            Thumbnail = user.PhotoKey != null
                ? Url.Action("GetFile", controller: "Photos", new { key = user.PhotoKey })
                : null,
        };

        // Возвращаем модель настроек
        return settingsModel;
    }

    /// <summary>
    /// Получает первую ошибку из ModelState.
    /// </summary>
    /// <returns>Сообщение об ошибке.</returns>
    private string GetFirstError()
    {
        // Выбирает все ошибки из коллекции ModelState.Values и объединяет их в одну коллекцию с помощью метода SelectMany.
        // Затем берет первую ошибку с помощью метода First и возвращает сообщение об ошибке из свойства ErrorMessage первой ошибки.
        return ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
    }
}