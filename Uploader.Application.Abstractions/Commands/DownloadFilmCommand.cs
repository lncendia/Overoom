using MediatR;
using Uploader.Application.Abstractions.DTOs;

namespace Uploader.Application.Abstractions.Commands;

/// <summary>
/// Команда для непосредственного скачивания фильма
/// </summary>
public class DownloadFilmCommand : IRequest
{
    /// <summary>
    /// Magnet-ссылка для скачивания через торрент-клиент
    /// </summary>
    public required string MagnetUri { get; init; }
    
    /// <summary>
    /// Имя файла для обработки
    /// </summary>
    public string? Filename { get; init; }

    /// <summary>
    /// Метаданные фильма для обработки и идентификации
    /// </summary>
    public required FilmRecord FilmRecord { get; init; }
}