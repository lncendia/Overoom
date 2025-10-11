namespace Films.Application.Abstractions.DTOs.Common;

/// <summary>
/// DTO для передачи файловых данных
/// </summary>
public class FileDto
{
    /// <summary>
    /// Поток данных файла
    /// </summary>
    public required Stream File { get; init; }
    
    /// <summary>
    /// MIME-тип содержимого файла
    /// </summary>
    public required string ContentType { get; init; }
}