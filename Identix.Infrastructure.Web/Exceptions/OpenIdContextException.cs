namespace Identix.Infrastructure.Web.Exceptions;

/// <summary>
/// Исключение, возникающее при отсутствии контекста OpenID.
/// </summary>
public class OpenIdContextException() : Exception("Action is not possible without an OpenID context");