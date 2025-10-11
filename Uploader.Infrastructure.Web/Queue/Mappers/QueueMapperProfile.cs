using Uploader.Application.Abstractions.Commands;
using Uploader.Application.Abstractions.DTOs;
using Uploader.Infrastructure.Web.Queue.InputModels;

namespace Uploader.Infrastructure.Web.Queue.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с очередью в команды
/// </summary>
public class QueueMapperProfile : AutoMapper.Profile
{
    /// <summary>
    /// Маппинг входных моделей в команды
    /// </summary>
    public QueueMapperProfile()
    {
        // Карта для QueueInputModel в EnqueueDownloadCommand
        CreateMap<QueueInputModel, EnqueueDownloadCommand>()
            .ForMember(r => r.FilmRecord, opt => opt.MapFrom(s => s));

        CreateMap<QueueInputModel, FilmRecord>()
            .ForMember(r => r.Id, opt => opt.MapFrom(s => s.FilmId));
    }
}