using MediatR;

namespace Identix.Application.Abstractions.Queries;

/// <summary>
/// Команда для получения фото пользователя
/// </summary>
/// <param name="Key">Ключ фото</param>
public record PhotoQuery(string Key) : IRequest<FileResult>;

/// <summary>
/// Результат получения файла из S3
/// </summary>
/// <param name="Stream">Поток с содержимым файла</param>
/// <param name="ContentType">MIME-тип содержимого</param>
/// <param name="FileName">Имя файла для загрузки</param>
public record FileResult(Stream Stream, string ContentType, string FileName);