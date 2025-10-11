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
    public ConsentController(ISender mediator, IStringLocalizer<ConsentController> stringLocalizer, ILogger<ConsentController> logger)
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
        // Получаем сохраненный OIDC-запрос из сессии по returnUrl
        var context = HttpContext.Session.GetOpenIdRequest(returnUrl);

        // Если контекст не найден — выбрасываем исключение
        if (context == null) throw new OpenIdContextException();

        // Строим ViewModel для страницы согласия
        var vm = await CreateConsentViewModelAsync(returnUrl, context);

        // Передаем ViewModel в представление
        return View(vm);
    }

    /// <summary>
    /// Обрабатывает обратную передачу экрана согласия
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConsentInputModel model)
    {
        // Проверяем, находимся ли мы в контексте запроса авторизации
        var context = HttpContext.Session.GetOpenIdRequest(model.ReturnUrl);

        // Если у нас нет действительного контекста - вызываем исключение
        if (context == null) throw new OpenIdContextException();

        // Устанавливаем в строку запроса закодированную returnUrl, чтоб при изменении локали открылась корректная ссылка (смотреть _Culture.cshtml)
        HttpContext.Request.QueryString = new QueryString("?ReturnUrl=" + HttpUtility.UrlEncode(model.ReturnUrl));

        // Если пользователь не согласовал ни одну область
        if (model.ScopesConsented.Count == 0)
        {
            // Добавляем ошибку в состояние модели
            ModelState.AddModelError("", _stringLocalizer["NoOneConsented"]);

            // Строим модель
            var vm = await BuildViewModelAsync(model, context);

            // Возвращаем представление
            return View(vm);
        }

        // Обрабатываем согласие
        ProcessConsent(model, context);

        // Перенаправляем назад в клиента
        return Redirect(model.ReturnUrl);
    }

    /// <summary>
    /// Обрабатывает отказ пользователя от согласия (consent denial)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DenyConsent(string returnUrl = "/")
    {
        // Получаем сохраненный контекст OIDC-запроса из сессии по returnUrl
        var context = HttpContext.Session.GetOpenIdRequest(returnUrl);

        // Если контекст не найден — выбрасываем исключение
        if (context == null) throw new OpenIdContextException();

        // Создаем объект ConsentResponse, фиксируя, что пользователь отклонил согласие
        HttpContext.Session.DenyConsent(context, User.GetId());

        // Логируем событие отказа от согласия
        _logger.LogInformation(
            "User {UserId} denied consent to client {ClientId}. " +
            "Requested scopes: {RequestedScopes}",
            User.Id(),
            context.ClientId,
            string.Join(", ", context.GetScopes()));

        // Перенаправляем пользователя на исходный URL возврата
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


        // Отправляем результат согласия в сессию
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
        // Создаем базовую ViewModel согласия с клиентом и scope'ами
        var consent = await CreateConsentViewModelAsync(model.ReturnUrl, context);

        // Устанавливаем флаг "запомнить согласие", если пользователь его отметил
        consent.RememberConsent = model.RememberConsent;

        // Устанавливаем дополнительное описание, введенное пользователем
        consent.Description = model.Description;

        // Снимаем отметки с Identity scope'ов, которые пользователь убрал
        foreach (var consentIdentityScope in consent.IdentityScopes
                     .Where(consentIdentityScope => !model.ScopesConsented.Contains(consentIdentityScope.Value)))
        {
            consentIdentityScope.Checked = false;
        }

        // Снимаем отметки с API scope'ов, которые пользователь убрал
        foreach (var consentApiScope in consent.ApiScopes
                     .Where(consentApiScope => !model.ScopesConsented.Contains(consentApiScope.Value)))
        {
            consentApiScope.Checked = false;
        }

        // Возвращаем полностью подготовленную модель для отображения в UI
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
        // Получаем данные клиента по ClientId
        var clientQuery = new ClientQuery { ClientId = request.ClientId! };
        var client = await _mediator.Send(clientQuery);

        // Утилита локализации запроса
        var requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
        
        // Получаем список scope'ов (Identity и API) с учетом текущей UI-культуры
        var scopesQuery = new ScopesQuery
        {
            RequestedScopes = request.GetScopes(),
            Culture = requestCultureFeature!.RequestCulture.UICulture
        };
        var scopes = await _mediator.Send(scopesQuery);

        // Формируем модель представления для страницы согласия
        return new ConsentViewModel
        {
            // URL возврата после согласия/отказа
            ReturnUrl = returnUrl,

            // Отображаемое имя клиента (или ClientId, если имя отсутствует)
            ClientName = client.ClientName,

            // Ссылка на сайт клиента (если указана)
            ClientUrl = client.ClientUrl,

            // Ссылка на логотип клиента (если указана)
            ClientLogoUrl = client.ClientLogoUrl,

            // Identity scopes
            IdentityScopes = scopes
                .Where(s => s.IdentityScope)
                .Select(CreateScopeViewModel),

            // API scopes
            ApiScopes = scopes
                .Where(s => !s.IdentityScope)
                .Select(CreateScopeViewModel),
        };
    }

    /// <summary>
    /// Преобразует объект Scope в ScopeViewModel для отображения на странице согласия
    /// </summary>
    /// <param name="scope">Scope из базы/сервиса</param>
    /// <returns>ViewModel для конкретного scope</returns>
    private static ScopeViewModel CreateScopeViewModel(ScopeDto scope) => new()
    {
        // Уникальное имя scope
        Value = scope.Name,

        // Отображаемое имя scope
        DisplayName = scope.DisplayName,

        // Описание scope (если есть)
        Description = scope.Description,

        // Подчеркивание в UI (акцент на scope)
        Emphasize = scope.Emphasize,

        // Является ли scope обязательным
        Required = scope.Required,

        // Отмечен ли scope пользователем (checked)
        Checked = scope.Checked,
    };
}