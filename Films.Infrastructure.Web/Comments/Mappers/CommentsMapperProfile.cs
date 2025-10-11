using Films.Application.Abstractions.Queries.Comments;
using Films.Infrastructure.Web.Comments.InputModels;

namespace Films.Infrastructure.Web.Comments.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с комментариями в команды
/// </summary>
public class CommentsMapperProfile : AutoMapper.Profile
{
    /// <summary>
    /// Маппинг входных моделей в команды
    /// </summary>
    public CommentsMapperProfile()
    {
        // Карта для GetCommentsInputModel в GetFilmCommentsQuery
        CreateMap<GetCommentsInputModel, GetFilmCommentsQuery>();
    }
}