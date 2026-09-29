using Common.Application.Transactions;
using MediatR;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Identix.Application.Abstractions.Commands.Profile;
using Identix.Application.Abstractions.Enums;
using Identix.Application.Abstractions.Extensions;
using Identix.Infrastructure.Web.Extensions;

namespace Identix.Infrastructure.Web.Culture.Controllers;

/// <summary>
/// Контроллер для изменения настроек культуры
/// </summary>
public class CultureController(ISender mediator) : Controller
{
  /// <summary>
  /// Метод устанавливает куки с запрошенной культурой
  /// </summary>
  /// <param name="culture">Название культуры</param>
  /// <param name="returnUrl">Адрес на который нужно вернуться</param>
  [HttpPost]
  [Transactional]
  public async Task<IActionResult> SetCulture(string culture, string returnUrl = "/")
  {
    Localization localization = culture.GetLocalization();

    if (User is { Identity.IsAuthenticated: true })
    {
      await mediator.Send(new ChangeLocaleCommand
      {
        UserId = User.Id(),

        Localization = localization
      });
    }

    Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
      CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(localization.GetLocalizationString())),
      new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

    return LocalRedirect(returnUrl);
  }
}