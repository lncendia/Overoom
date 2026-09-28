using Films.Application.Abstractions.Commands.Rooms;
using Films.Application.Abstractions.Queries.Rooms;
using Films.Infrastructure.Web.Rooms.InputModels;

namespace Films.Infrastructure.Web.Rooms.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с комнатами в команды
/// </summary>
public class RoomsMapperProfile : AutoMapper.Profile
{
  /// <summary>
  /// Маппинг входных моделей в команды
  /// </summary>
  public RoomsMapperProfile()
  {
    CreateMap<CreateRoomInputModel, CreateRoomCommand>();
    CreateMap<SearchRoomsInputModel, SearchRoomsQuery>();
  }
}