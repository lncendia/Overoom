using MediatR;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Identix.Application.Abstractions.Commands.Profile;
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
    public async Task<IActionResult> SetCulture(string culture, string returnUrl = "/")
    {
        // Получаем локализацию, если не найдена, то будет выдана локализация по умолчанию - en
        var localization = culture.GetLocalization();

        // Если пользователь аутентифицирован, выполняем следующий блок кода.
        if (User is { Identity.IsAuthenticated: true })
        {
            // Отправляем команду на изменение локализации с указанием идентификатора пользователя и локализации.
            await mediator.Send(new ChangeLocaleCommand
            {
                // Идентификатор пользователя
                UserId = User.Id(),
                
                // Локализация
                Localization = localization
            });
        }
        
        // Устанавливаем куку с информацией о языке
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(localization.GetLocalizationString())),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

        // Перенаправляем на локальный URL
        return LocalRedirect(returnUrl);
    }
}