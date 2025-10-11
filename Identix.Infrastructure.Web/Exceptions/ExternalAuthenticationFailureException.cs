namespace Identix.Infrastructure.Web.Exceptions;

/// <summary>
/// Исключение, возникающее при неудачной аутентификации
/// </summary>
public class ExternalAuthenticationFailureException(string message) : Exception(message);