using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Identix.Application.Abstractions;
using Identix.Infrastructure.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Client.WebIntegration;

namespace Identix.Infrastructure.Web.External.Services;

/// <summary>
/// Маппер claims для провайдера GitHub
/// </summary>
public class GitHubClaimsMapper : ExternalClaimsMapperBase
{
  /// <summary>
  /// Endpoint GitHub API для получения email-адресов пользователя
  /// </summary>
  private const string EmailsEndpoint = "https://api.github.com/user/emails";

  /// <summary>
  /// HTTP context accessor для доступа к текущему контексту запроса
  /// </summary>
  private readonly IHttpContextAccessor _httpContextAccessor;

  /// <summary>
  /// HTTP client для выполнения внешних запросов к GitHub API
  /// </summary>
  private readonly HttpClient _backchannel;

  /// <summary>
  /// Логгер для записи событий и ошибок
  /// </summary>
  private readonly ILogger<GitHubClaimsMapper> _logger;

  /// <summary>
  /// Конструктор маппера claims для провайдера GitHub
  /// </summary>
  /// <param name="httpContextAccessor">Доступ к HTTP контексту</param>
  /// <param name="backchannel">HTTP клиент для внешних запросов</param>
  /// <param name="logger">Логгер для записи событий</param>
  public GitHubClaimsMapper(IHttpContextAccessor httpContextAccessor, HttpClient backchannel,
    ILogger<GitHubClaimsMapper> logger) : base(OpenIddictClientWebIntegrationConstants.Providers.GitHub)
  {
    _httpContextAccessor = httpContextAccessor;
    _backchannel = backchannel;
    _logger = logger;
  }

  /// <summary>
  /// Основной метод маппинга claims из GitHub в стандартные claims системы
  /// </summary>
  /// <param name="result">Результат аутентификации от GitHub</param>
  /// <returns>Identity с маппированными claims</returns>
  /// <exception cref="Exception">Выбрасывается при отсутствии обязательных данных</exception>
  /// <exception cref="HttpRequestException">Выбрасывается при ошибке запроса к GitHub API</exception>
  public override async Task<ClaimsIdentity> MapAsync(AuthenticateResult result)
  {
    string id = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("GitHub: missing id");

    ClaimsIdentity identity = CreateBaseIdentity(id);
    identity.TryAddClaim(ClaimTypes.Name, result.Principal?.FindFirstValue("login"));
    identity.TryAddClaim(Constants.Claims.Thumbnail, result.Principal?.FindFirstValue("avatar_url"));
    IEnumerable<AuthenticationToken>? tokens = result.Properties?.GetTokens();
    string? accessToken = tokens?.FirstOrDefault(t => t.Name == "backchannel_access_token")?.Value;

    if (string.IsNullOrEmpty(accessToken))
      throw new Exception("GitHub: missing access_token");

    var request = new HttpRequestMessage(HttpMethod.Get, EmailsEndpoint);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    // Добавляем User-Agent, так как GitHub требует его для всех API запросов
    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Identix", "1.0.0"));
    CancellationToken cancellation = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
    using HttpResponseMessage response = await _backchannel.SendAsync(request, cancellation);

    if (!response.IsSuccessStatusCode)
    {
      _logger.LogError("GitHub email request failed: {StatusCode} {Reason}",
        response.StatusCode, response.ReasonPhrase);
      throw new HttpRequestException("An error occurred while retrieving the GitHub user emails.");
    }

    using var container = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellation));

    string? email = container.RootElement
      .EnumerateArray()
      .Where(e => e.TryGetProperty("primary", out JsonElement p) && p.GetBoolean())
      .Where(e => e.TryGetProperty("verified", out JsonElement v) && v.GetBoolean())
      .Select(e => e.GetProperty("email").GetString())
      .FirstOrDefault();

    if (!string.IsNullOrEmpty(email))
      identity.TryAddClaim(ClaimTypes.Email, email);

    return identity;
  }
}