using Films.Application.Abstractions.Queries.Films;
using Films.Infrastructure.Web.Films.InputModels;

namespace Films.Infrastructure.Web.Films.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с фильмами в команды
/// </summary>
public class FilmsMapperProfile : AutoMapper.Profile
{
    /// <summary>
    /// Маппинг входных моделей в команды
    /// </summary>
    public FilmsMapperProfile()
    {
        // Карта для GetPopularFilmsInputModel в GetPopularFilmsQuery
        CreateMap<GetPopularFilmsInputModel, GetPopularFilmsQuery>();
        
        // Карта для SearchFilmsInputModel в SearchFilmsQuery
        CreateMap<SearchFilmsInputModel, SearchFilmsQuery>();
    }
}