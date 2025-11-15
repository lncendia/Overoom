namespace Common.Application.EmailService;

/// <summary>
/// Интерфейс посетителя Email
/// </summary>
public interface IEmailVisitor
{
    /// <summary>
    /// Тема письма
    /// </summary>
    string? Subject { get; }
    
    /// <summary>
    /// Контент письма
    /// </summary>
    string? Body { get; }
}