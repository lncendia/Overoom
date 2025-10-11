using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Identix.Application.Abstractions.Commands.OpenId;
using Identix.Application.Abstractions.Queries;
using Identix.Infrastructure.Web.Attributes;
using Identix.Infrastructure.Web.Grants.ViewModels;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.Grants.Controllers;

/// <summary>
/// Контроллер для управления грантами (разрешениями).
/// </summary>
[Authorize]
[SecurityHeaders]
public class GrantsController : Controller
{
    /// <summary>
    /// Медиатор
    /// </summary>
    private readonly ISender _mediator;
    
    /// <summary>
    /// Логгер
    /// </summary>
    private readonly ILogger<GrantsController> _logger;

    /// <summary>
    /// Конструктор контроллера GrantsController.
    /// </summary>
    /// <param name="mediator">Медиатор</param>
    /// <param name="logger">Логгер</param>
    public GrantsController(ISender mediator, ILogger<GrantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Получить список разрешений
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await BuildViewModelAsync());
    }

    /// <summary>
    /// Обработка отзыва гранта (авторизации) по идентификатору
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(string grantId)
    {
        // Формируем команду для отзыва гранта конкретного пользователя
        var revokeCommand = new RevokeGrantCommand
        {
            UserId = User.Id(),
            GrantId = grantId
        };
        
        // Отправляем команду через CQRS-пайплайн
        await _mediator.Send(revokeCommand);

        // Логируем успешный отзыв гранта
        _logger.LogInformation(
            "User {UserId} revoked the grant {GrantId}",
            revokeCommand.UserId,
            revokeCommand.GrantId);
        
        // После отзыва перенаправляем обратно на список грантов
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Построить модель представления для страницы "Grants"
    /// </summary>
    private async Task<GrantsViewModel> BuildViewModelAsync()
    {
        // Утилита локализации запроса
        var requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();

        // Создание запроса разрешений пользователя
        var grantsQuery = new UserGrantsQuery
        {
            UserId = User.Id(),
            Culture = requestCultureFeature!.RequestCulture.UICulture
        };

        // Получение всех разрешений пользователя
        var grants = await _mediator.Send(grantsQuery);

        // Создание списка моделей представления разрешений
        var grantsViewModels = new List<GrantViewModel>();

        // Итерация по каждому разрешению
        foreach (var grant in grants)
        {
            // Создание нового объекта GrantViewModel 
            var item = new GrantViewModel
            {
                // Установка идентификатора клиента в свойство ClientId 
                GrantId = grant.Id,

                // Установка имени клиента в свойство ClientName, если оно не равно null, иначе установка ClientId 
                ClientName = grant.ClientName,

                // Установка URL клиента в свойство ClientUrl 
                ClientUrl = grant.ClientUrl,

                // Установка URL логотипа клиента в свойство ClientLogoUrl 
                ClientLogoUrl = grant.ClientLogoUrl,

                // Установка описания в свойство Description 
                Description = grant.Description,

                // Установка даты создания в свойство Created 
                Created = grant.Created,

                // Установка имен идентификационных разрешений в свойство IdentityGrantNames 
                IdentityGrantNames =
                    grant.Scopes.Where(s => s.IdentityScope).Select(s => s.Description ?? s.DisplayName),

                // Установка имен разрешений API в свойство ApiGrantNames 
                ApiGrantNames = grant.Scopes.Where(s => !s.IdentityScope).Select(s => s.Description ?? s.DisplayName)
            };

            // Добавление модели представления разрешения в список
            grantsViewModels.Add(item);
        }

        // Возвращение модели представления со списком разрешений
        return new GrantsViewModel
        {
            // Установка списка разрешений
            Grants = grantsViewModels
        };
    }
}