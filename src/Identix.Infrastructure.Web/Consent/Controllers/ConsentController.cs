using System.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Queries;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Consent.InputModels;
using Identix.Infrastructure.Web.Consent.ViewModels;
using Identix.Infrastructure.Web.Exceptions;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.Consent.Controllers;

/// <summary>
/// Этот контроллер обрабатывает пользовательский интерфейс согласия
/// </summary>
[Authorize]
[SecurityHeaders]
public class ConsentController : Controller
{
  /// <summary>
  /// Локализатор
  /// </summary>
  private readonly IStringLocalizer<ConsentController> _stringLocalizer;

  /// <summary>
  /// Медиатор
  /// </summary>
  private readonly ISender _mediator;

  /// <summary>
  /// Логгер
  /// </summary>
  private readonly ILogger<ConsentController> _logger;

  /// <summary>
  /// Конструктор класса ConsentController.
  /// </summary>
  /// <param name="mediator">Медиатор</param>
  /// <param name="stringLocalizer">Локализатор</param>
  /// <param name="logger">Логгер</param>
  public ConsentController(ISender mediator, IStringLocalizer<ConsentController> stringLocalizer,
    ILogger<ConsentController> logger)
  {
    _mediator = mediator;
    _stringLocalizer = stringLocalizer;
    _logger = logger;
  }

  /// <summary>
  /// Отображает страницу согласия пользователя (consent page)
  /// </summary>
  /// <param name="returnUrl">URL возврата после согласия/отказа</param>
  [HttpGet]
  public async Task<IActionResult> Index(string returnUrl = "/")
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(returnUrl);
    if (context == null) throw new OpenIdContextException();

    ConsentViewModel vm = await CreateConsentViewModelAsync(returnUrl, context);

    return View(vm);
  }

  /// <summary>
  /// Обрабатывает обратную передачу экрана согласия
  /// </summary>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Index(ConsentInputModel model)
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(model.ReturnUrl);
    if (context == null) throw new OpenIdContextException();

    HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

    if (model.ScopesConsented.Count == 0)
    {
      ModelState.AddModelError("", _stringLocalizer["NoOneConsented"]);
      ConsentViewModel vm = await BuildViewModelAsync(model, context);

      return View(vm);
    }

    ProcessConsent(model, context);

    return Redirect(model.ReturnUrl);
  }

  /// <summary>
  /// Обрабатывает отказ пользователя от согласия (consent denial)
  /// </summary>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult DenyConsent(string returnUrl = "/")
  {
    OpenIddictRequest? context = HttpContext.Session.GetOpenIdRequest(returnUrl);
    if (context == null) throw new OpenIdContextException();

    HttpContext.Session.DenyConsent(context, User.GetId());

    _logger.LogInformation(
      "User {UserId} denied consent to client {ClientId}. " +
      "Requested scopes: {RequestedScopes}",
      User.Id(),
      context.ClientId,
      string.Join(", ", context.GetScopes()));

    return Redirect(returnUrl);
  }

  /// <summary>
  /// Обработка согласия
  /// </summary>
  /// <param name="model">Модель данных, отправленных пользователем из формы (ConsentInputModel)</param>
  /// <param name="context">OIDC-запрос авторизации (OpenIddictRequest)</param>
  private void ProcessConsent(ConsentInputModel model, OpenIddictRequest context)
  {
    var grantedConsent = new OpenIdExtensions.ConsentResponse
    {
      RememberConsent = model.RememberConsent,
      Description = model.Description,
      GrantedScopes = model.ScopesConsented
    };

    _logger.LogInformation(
      "User {UserId} granted consent to client {ClientId}. " +
      "Requested scopes: {RequestedScopes}. " +
      "Granted scopes: {GrantedScopes}. " +
      "Remember consent: {RememberConsent}",
      User.Id(),
      context.ClientId,
      string.Join(", ", context.GetScopes()),
      string.Join(", ", grantedConsent.GrantedScopes),
      grantedConsent.RememberConsent);


    HttpContext.Session.GrantConsent(context, User.GetId(), grantedConsent);
  }

  /// <summary>
  /// Создает и подготавливает ViewModel согласия пользователя на основе данных формы и контекста авторизации
  /// </summary>
  /// <param name="model">Модель данных, отправленных пользователем из формы (ConsentInputModel)</param>
  /// <param name="context">OIDC-запрос авторизации (OpenIddictRequest)</param>
  /// <returns>Заполненная модель представления для страницы согласия</returns>
  private async Task<ConsentViewModel> BuildViewModelAsync(ConsentInputModel model, OpenIddictRequest context)
  {
    ConsentViewModel consent = await CreateConsentViewModelAsync(model.ReturnUrl, context);
    consent.RememberConsent = model.RememberConsent;
    consent.Description = model.Description;

    foreach (ScopeViewModel consentIdentityScope in consent.IdentityScopes
               .Where(consentIdentityScope => !model.ScopesConsented.Contains(consentIdentityScope.Value)))
    {
      consentIdentityScope.Checked = false;
    }

    foreach (ScopeViewModel consentApiScope in consent.ApiScopes
               .Where(consentApiScope => !model.ScopesConsented.Contains(consentApiScope.Value)))
    {
      consentApiScope.Checked = false;
    }

    return consent;
  }

  /// <summary>
  /// Создает ViewModel для страницы согласия пользователя (consent page)
  /// </summary>
  /// <param name="returnUrl">URL, на который нужно вернуться после согласия/отказа</param>
  /// <param name="request">Запрос авторизации OpenIddict</param>
  /// <returns>Модель представления для страницы согласия</returns>
  private async Task<ConsentViewModel> CreateConsentViewModelAsync(string returnUrl, OpenIddictRequest request)
  {
    var clientQuery = new ClientQuery { ClientId = request.ClientId! };
    ClientDto client = await _mediator.Send(clientQuery);
    IRequestCultureFeature? requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();

    var scopesQuery = new ScopesQuery
    {
      RequestedScopes = request.GetScopes(),
      Culture = requestCultureFeature!.RequestCulture.UICulture
    };
    IReadOnlyList<ScopeDto> scopes = await _mediator.Send(scopesQuery);

    return new ConsentViewModel
    {
      ReturnUrl = returnUrl,

      ClientName = client.ClientName,

      ClientUrl = client.ClientUrl,

      ClientLogoUrl = client.ClientLogoKey != null
        ? Url.Action("GetFile", "Photos", new { key = client.ClientLogoKey })
        : null,

      IdentityScopes = scopes
        .Where(s => s.IdentityScope)
        .Select(CreateScopeViewModel),

      ApiScopes = scopes
        .Where(s => !s.IdentityScope)
        .Select(CreateScopeViewModel)
    };
  }

  /// <summary>
  /// Преобразует объект Scope в ScopeViewModel для отображения на странице согласия
  /// </summary>
  /// <param name="scope">Scope из базы/сервиса</param>
  /// <returns>ViewModel для конкретного scope</returns>
  private static ScopeViewModel CreateScopeViewModel(ScopeDto scope)
  {
    return new ScopeViewModel
    {
      Value = scope.Name,

      DisplayName = scope.DisplayName,

      Description = scope.Description,

      Emphasize = scope.Emphasize,

      Required = scope.Required,

      Checked = scope.Checked
    };
  }
}