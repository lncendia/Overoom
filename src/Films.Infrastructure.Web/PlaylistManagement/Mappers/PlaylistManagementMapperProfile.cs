using Films.Application.Abstractions.Commands.Playlists;
using Films.Infrastructure.Web.PlaylistManagement.InputModels;

namespace Films.Infrastructure.Web.PlaylistManagement.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с подборками в команды
/// </summary>
public class PlaylistManagementMapperProfile : AutoMapper.Profile
{
  /// <summary>
  /// Маппинг входных моделей в команды
  /// </summary>
  public PlaylistManagementMapperProfile()
  {
    CreateMap<CreatePlaylistInputModel, CreatePlaylistCommand>();
    CreateMap<ChangePlaylistInputModel, ChangePlaylistCommand>();
    CreateMap<ChangePlaylistPosterInputModel, ChangePlaylistPosterCommand>();
  }
}