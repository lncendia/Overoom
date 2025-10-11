using Uploader.Application.Abstractions.Commands;
using Hangfire;
using MediatR;
using Uploader.Application.Abstractions;

namespace Uploader.Application.Services.Commands;

/// <summary>
/// Обработчик команды добавления комментария
/// </summary>
/// <param name="backgroundJobClient">Клиент для постановки фоновых задач.</param>
public class EnqueueDownloadCommandHandler(IBackgroundJobClientV2 backgroundJobClient)
    : IRequestHandler<EnqueueDownloadCommand>
{
    /// <summary>
    /// Обрабатывает команду добавления нового комментария
    /// </summary>
    /// <param name="request">Команда с данными для создания комментария</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO созданного комментария</returns>
    public Task Handle(EnqueueDownloadCommand request, CancellationToken cancellationToken)
    {
        var command = new DownloadFilmCommand
        {
            FilmRecord = request.FilmRecord,
            MagnetUri = request.MagnetUri,
            Filename = request.Filename
        };

        backgroundJobClient.Enqueue<ISender>(Constants.Hangfire.Queue, s => s.Send(command, CancellationToken.None));
        return Task.CompletedTask;
    }
}