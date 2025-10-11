namespace Common.Application.EmailService;

/// <summary>
/// Интерфейс посетителя Email
/// Определяет базовый контракт для всех типов email-сообщений в системе
/// Реализует паттерн "Посетитель" для обработки различных типов email
/// </summary>
public interface IEmailVisitor
{
    /// <summary>
    /// Тема письма
    /// Заголовок email сообщения, отображаемый получателю
    /// </summary>
    string? Subject { get; }
    
    /// <summary>
    /// Контент письма
    /// Тело email сообщения, может содержать HTML или plain text
    /// </summary>
    string? Body { get; }
}