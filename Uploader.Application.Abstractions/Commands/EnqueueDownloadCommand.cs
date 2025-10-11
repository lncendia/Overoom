using MediatR;
using Uploader.Application.Abstractions.DTOs;

namespace Uploader.Application.Abstractions.Commands;

/// <summary>
/// Команда для постановки задачи скачивания в очередь
/// </summary>
public class EnqueueDownloadCommand : IRequest
{
    /// <summary>
    /// Magnet-ссылка для скачивания через торрент-клиент
    /// </summary>
    public required string MagnetUri { get; init; }
    
    /// <summary>
    /// Имя файла для обработки (если уже доступен локально)
    /// </summary>
    public string? Filename { get; init; }
    
    /// <summary>
    /// Метаданные фильма для обработки и идентификации
    /// </summary>
    public required FilmRecord FilmRecord { get; init; }
}