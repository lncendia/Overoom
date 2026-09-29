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
    var revokeCommand = new RevokeGrantCommand
    {
      UserId = User.Id(),
      GrantId = grantId
    };

    await _mediator.Send(revokeCommand);

    _logger.LogInformation(
      "User {UserId} revoked the grant {GrantId}",
      revokeCommand.UserId,
      revokeCommand.GrantId);

    return RedirectToAction("Index");
  }

  /// <summary>
  /// Построить модель представления для страницы "Grants"
  /// </summary>
  private async Task<GrantsViewModel> BuildViewModelAsync()
  {
    IRequestCultureFeature? requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();

    var grantsQuery = new UserGrantsQuery
    {
      UserId = User.Id(),
      Culture = requestCultureFeature!.RequestCulture.UICulture
    };

    IReadOnlyList<GrantDto> grants = await _mediator.Send(grantsQuery);
    var grantsViewModels = new List<GrantViewModel>();

    foreach (GrantDto grant in grants)
    {
      var item = new GrantViewModel
      {
        GrantId = grant.Id,

        ClientName = grant.ClientName,

        ClientUrl = grant.ClientUrl,

        ClientLogoUrl = grant.ClientLogoKey != null
          ? Url.Action("GetFile", "Photos", new { key = grant.ClientLogoKey })
          : null,

        Description = grant.Description,

        Created = grant.Created,

        IdentityGrantNames =
          grant.Scopes.Where(s => s.IdentityScope).Select(s => s.Description ?? s.DisplayName),

        ApiGrantNames = grant.Scopes.Where(s => !s.IdentityScope).Select(s => s.Description ?? s.DisplayName)
      };

      grantsViewModels.Add(item);
    }

    return new GrantsViewModel
    {
      Grants = grantsViewModels
    };
  }
}