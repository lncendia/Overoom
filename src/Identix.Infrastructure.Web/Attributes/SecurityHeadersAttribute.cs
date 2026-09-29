using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Identix.Infrastructure.Web.Attributes;

/// <summary>
/// Добавляет заголовки безопасности (csp, referer и т.д.)
/// </summary>
public class SecurityHeadersAttribute : ActionFilterAttribute
{
  ///<summary>
  /// Метод OnResultExecuting вызывается перед выполнением результата действия контроллера.
  ///</summary>
  ///<param name="context">Контекст выполнения результата.</param>
  public override void OnResultExecuting(ResultExecutingContext context)
  {
    IActionResult result = context.Result;
    if (result is not ViewResult) return;

    byte[] nonceBytes = RandomNumberGenerator.GetBytes(16);
    string nonce = Convert.ToBase64String(nonceBytes);
    context.HttpContext.Items["CSP-Nonce"] = nonce;

    if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
    {
      context.HttpContext.Response.Headers.XContentTypeOptions = "nosniff";
    }

    if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
    {
      context.HttpContext.Response.Headers.XFrameOptions = "SAMEORIGIN";
    }

    string csp =
      "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";

    csp += "upgrade-insecure-requests;";
    csp += $"script-src 'self' 'nonce-{nonce}';";
    csp += "img-src 'self' data:;";
    csp += "style-src 'self' 'unsafe-inline'";

    if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
    {
      context.HttpContext.Response.Headers.ContentSecurityPolicy = csp;
    }

    if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
    {
      context.HttpContext.Response.Headers["X-Content-Security-Policy"] = csp;
    }

    const string referrerPolicy = "no-referrer";
    if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
    {
      context.HttpContext.Response.Headers["Referrer-Policy"] = referrerPolicy;
    }
  }
}
