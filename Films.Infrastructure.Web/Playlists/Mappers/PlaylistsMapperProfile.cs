using Films.Application.Abstractions.Queries.Playlists;
using Films.Infrastructure.Web.Playlists.InputModels;

namespace Films.Infrastructure.Web.Playlists.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с подборками в команды
/// </summary>
public class PlaylistsMapperProfile : AutoMapper.Profile
{
    /// <summary>
    /// Маппинг входных моделей в команды
    /// </summary>
    public PlaylistsMapperProfile()
    {
        // Карта для PlaylistsSearchInputModel в SearchPlaylistsQuery
        CreateMap<SearchPlaylistsInputModel, SearchPlaylistsQuery>();
    }
}