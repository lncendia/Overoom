using Common.Application.Transactions;
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
using Identix.Application.Abstractions.Enums;
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
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(returnUrl);
    RegistrationViewModel vm = await BuildRegisterViewModelAsync(returnUrl, context);

    return View(vm);
  }

  /// <summary>
  /// Обработка регистрации пользователя
  /// </summary>
  /// <param name="model">Модель входа в систему</param>
  [HttpPost]
  [ValidateAntiForgeryToken]
  [AllowAnonymous]
  [Transactional]
  public async Task<IActionResult> Registration(RegistrationInputModel model)
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(model.ReturnUrl);
    HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

    if (!ModelState.IsValid)
    {
      RegistrationViewModel vm = await BuildRegisterViewModelAsync(model, context);
      return View(vm);
    }

    string callbackUrl = Url.Action("ConfirmEmail", "Registration", null, HttpContext.Request.Scheme)!;
    IRequestCultureFeature? requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
    Localization locale = requestCulture!.RequestCulture.UICulture.Name.GetLocalization();

    try
    {
      AppUser user = await _mediator.Send(new CreateUserCommand
      {
        Email = model.Email!,
        Password = model.Password!,
        ConfirmUrl = callbackUrl,
        Locale = locale,
        ReturnUrl = model.ReturnUrl
      });

      _logger.LogInformation(
        "User registration successful. Email: {Email}, UserId: {UserId}, UserName: {UserName}, ClientId: {ClientId}",
        user.Email, user.Id, user.UserName, context?.ClientId);

      return RedirectToAction("ConfirmEmailMailSent", "Account", new { returnUrl = model.ReturnUrl });
    }
    catch (Exception ex)
    {
      switch (ex)
      {
        case EmailAlreadyTakenException:
          ModelState.AddModelError("", _localizer["UserAlreadyExist"]);
          break;

        case EmailFormatException:
          ModelState.AddModelError("", _localizer["EmailFormatInvalid"]);
          break;

        case PasswordValidationException passwordValidationException:
          foreach (KeyValuePair<string, string> error in passwordValidationException.ValidationErrors)
          {
            ModelState.AddModelError("", _localizer[error.Key]);
          }

          break;

        default: throw;
      }

      RegistrationViewModel vm = await BuildRegisterViewModelAsync(model, context);

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
  [Transactional]
  public async Task<IActionResult> ConfirmEmail(Guid? id, string? code, string returnUrl = "/")
  {
    if (!id.HasValue) throw new QueryParameterMissingException(nameof(id));
    if (code == null) throw new QueryParameterMissingException(nameof(code));

    await _mediator.Send(new VerifyEmailCommand
    {
      UserId = id.Value,
      Code = code
    });

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
    IEnumerable<string> schemes = (await _signInManager.GetExternalAuthenticationSchemesAsync()).Select(s => s.Name);
    bool enableLocalIdentityProvider = true;

    if (context == null)
    {
      return new RegistrationViewModel
      {
        ReturnUrl = returnUrl,
        EnableLocalLogin = enableLocalIdentityProvider,
        ExternalProviders = [.. schemes]
      };
    }

    if (context.IdentityProvider != null)
    {
      if (context.IdentityProvider == Constants.IdentityProviders.Local)
      {
        schemes = [];
      }
      else
      {
        enableLocalIdentityProvider = false;
        schemes = schemes.Where(provider => provider == context.IdentityProvider);
      }
    }

    return new RegistrationViewModel
    {
      ReturnUrl = returnUrl,
      EnableLocalLogin = enableLocalIdentityProvider,
      ExternalProviders = [.. schemes],
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
    RegistrationViewModel vm = await BuildRegisterViewModelAsync(model.ReturnUrl, context);
    vm.Email = model.Email;
    vm.Password = model.Password;
    vm.PasswordConfirm = model.PasswordConfirm;

    return vm;
  }
}