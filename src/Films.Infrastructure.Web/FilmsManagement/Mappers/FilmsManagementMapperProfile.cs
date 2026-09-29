using Films.Application.Abstractions.Commands.Films;
using Films.Application.Abstractions.DTOs.Common;
using Films.Domain.Films.ValueObjects;
using Films.Infrastructure.Web.FilmsManagement.InputModels;

namespace Films.Infrastructure.Web.FilmsManagement.Mappers;

/// <summary>
/// Класс для маппинга входных моделей для работы с фильмами в команды
/// </summary>
public class FilmsManagementMapperProfile : AutoMapper.Profile
{
  /// <summary>
  /// Маппинг входных моделей в команды
  /// </summary>
  public FilmsManagementMapperProfile()
  {
    CreateMap<AddFilmInputModel, AddFilmCommand>();
    CreateMap<ChangeFilmInputModel, ChangeFilmCommand>();
    CreateMap<ChangeFilmPosterInputModel, ChangeFilmPosterCommand>();
    CreateMap<ActorInputModel, Actor>();

    CreateMap<IFormFile, FileDto>()
      .ForMember(f => f.File, opt => opt.MapFrom(form => form.OpenReadStream()))
      .ForMember(f => f.ContentType, opt => opt.MapFrom(form => form.ContentType));
  }
}