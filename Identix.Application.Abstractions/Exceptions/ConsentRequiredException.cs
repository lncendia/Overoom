namespace Identix.Application.Abstractions.Exceptions;

/// <summary>
/// Исключение, указывающее, что требуется согласие (consent) пользователя
/// </summary>
public class ConsentRequiredException : Exception
{
    /// <summary>
    /// Тип согласия, который требуется
    /// </summary>
    public string ConsentType { get; }

    /// <summary>
    /// Создает новое исключение требования согласия
    /// </summary>
    /// <param name="consentType">Тип согласия из OpenIddictConstants.ConsentTypes</param>
    public ConsentRequiredException(string consentType) : base("Consent required")
    {
        ConsentType = consentType;
    }
}