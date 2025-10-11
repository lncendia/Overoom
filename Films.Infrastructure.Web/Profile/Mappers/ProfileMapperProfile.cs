using Common.Domain.Rooms;
using Films.Application.Abstractions.Commands.Profile;
using Films.Infrastructure.Web.Profile.InputModels;

namespace Films.Infrastructure.Web.Profile.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с профилем в команды
/// </summary>
public class ProfileMapperProfile : AutoMapper.Profile
{
    /// <summary>
    /// Маппинг входных моделей в команды
    /// </summary>
    public ProfileMapperProfile()
    {
        // Карта для UpdateRoomSettingsInputModel в RoomSettings
        CreateMap<UpdateRoomSettingsInputModel, RoomSettings>();
        
        // Карта для UpdateRoomSettingsInputModel в UpdateRoomSettingsCommand
        CreateMap<UpdateRoomSettingsInputModel, UpdateRoomSettingsCommand>()
            .ForMember(dest => dest.Settings, opt => opt.MapFrom(s => s));
    }
}