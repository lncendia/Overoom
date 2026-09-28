using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Identix.Application.Abstractions;
using Identix.Infrastructure.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера VkId
/// </summary>
public class VkIdClaimsMapper : ExternalClaimsMapperBase
{
  /// <summary>
  /// Endpoint VK API для получения информации о пользователе
  /// </summary>
  private const string UserInformationEndpoint = "https://api.vk.com/method/users.get";

  /// <summary>
  /// Версия VK API, используемая для запросов
  /// </summary>
  private const string ApiVersion = "5.199";

  /// <summary>
  /// HTTP context accessor для доступа к текущему контексту запроса
  /// </summary>
  private readonly IHttpContextAccessor _httpContextAccessor;

  /// <summary>
  /// HTTP client для выполнения внешних запросов к VK API
  /// </summary>
  private readonly HttpClient _backchannel;

  /// <summary>
  /// Логгер для записи событий и ошибок
  /// </summary>
  private readonly ILogger<VkIdClaimsMapper> _logger;

  /// <summary>
  /// Конструктор маппера claims для провайдера VkId
  /// </summary>
  /// <param name="httpContextAccessor">Доступ к HTTP контексту</param>
  /// <param name="backchannel">HTTP клиент для внешних запросов</param>
  /// <param name="logger">Логгер для записи событий</param>
  public VkIdClaimsMapper(IHttpContextAccessor httpContextAccessor, HttpClient backchannel,
    ILogger<VkIdClaimsMapper> logger) : base(OpenIddictClientWebIntegrationConstants.Providers.VkId)
  {
    _httpContextAccessor = httpContextAccessor;
    _backchannel = backchannel;
    _logger = logger;
  }

  /// <summary>
  /// Основной метод маппинга claims из VK в стандартные claims системы
  /// </summary>
  /// <param name="result">Результат аутентификации от VK</param>
  /// <returns>Identity с маппированными claims</returns>
  /// <exception cref="Exception">Выбрасывается при отсутствии обязательных данных</exception>
  /// <exception cref="HttpRequestException">Выбрасывается при ошибке запроса к VK API</exception>
  public override async Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
  {
    string id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("VkId: missing id");

    ClaimsIdentity identity = CreateBaseIdentity(id);
    identity.TryAddClaim(ClaimTypes.Email, result.Principal?.FindFirstValue(ClaimTypes.Email));
    identity.TryAddClaim(Constants.Claims.Thumbnail, result.Principal?.FindFirstValue("avatar"));
    IEnumerable<AuthenticationToken>? tokens = result.Properties?.GetTokens();
    string? accessToken = tokens?.FirstOrDefault(t => t.Name == "backchannel_access_token")?.Value;

    if (string.IsNullOrEmpty(accessToken))
      throw new Exception("VkId: missing access_token");

    var parameters = new Dictionary<string, string?>
    {
      ["v"] = ApiVersion,
      ["fields"] = "first_name,last_name"
    };

    string address = QueryHelpers.AddQueryString(UserInformationEndpoint, parameters);
    var request = new HttpRequestMessage(HttpMethod.Get, address);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    CancellationToken cancellation = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
    using HttpResponseMessage response = await _backchannel.SendAsync(request, cancellation);

    if (!response.IsSuccessStatusCode)
    {
      _logger.LogError("VK profile request failed: {StatusCode} {Reason}",
        response.StatusCode, response.ReasonPhrase);
      throw new HttpRequestException("An error occurred while retrieving the VK user profile.");
    }

    using var container = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellation));

    JsonElement user = container.RootElement
      .GetProperty("response")
      .EnumerateArray()
      .FirstOrDefault();

    // Берём имя и фамилию отсюда, так как в UserInfo приходит английский вариант имени
    if (user.ValueKind == JsonValueKind.Object)
    {
      string? firstName = user.TryGetProperty("first_name", out JsonElement fn) ? fn.GetString() : null;
      string? lastName = user.TryGetProperty("last_name", out JsonElement ln) ? ln.GetString() : null;

      if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
      {
        string fullName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrEmpty(x)));
        identity.TryAddClaim(ClaimTypes.Name, fullName);
      }
    }

    return identity;
  }
}