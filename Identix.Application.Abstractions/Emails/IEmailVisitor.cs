using Common.Application.EmailService;

namespace Identix.Application.Abstractions.Emails;

/// <summary>
/// Интерфейс посетителя Email
/// </summary>
public interface IExtendedEmailVisitor : IEmailVisitor
{
    /// <summary>
    /// Посещает ConfirmRegistrationEmail.
    /// </summary>
    /// <param name="email">Объект ConfirmRegistrationEmail.</param>
    void Visit(ConfirmRegistrationEmail email);
    
    /// <summary>
    /// Посещает ConfirmRecoverPasswordEmail.
    /// </summary>
    /// <param name="email">Объект ConfirmRecoverPasswordEmail.</param>
    void Visit(ConfirmRecoverPasswordEmail email);
    
    /// <summary>
    /// Посещает ConfirmMailChangeEmail.
    /// </summary>
    /// <param name="email">Объект ConfirmMailChangeEmail.</param>
    void Visit(ConfirmMailChangeEmail email);

    /// <summary>
    /// Посещает TwoFactorCodeEmail.
    /// </summary>
    /// <param name="email">Объект TwoFactorCodeEmail.</param>
    void Visit(TwoFactorCodeEmail email);
}