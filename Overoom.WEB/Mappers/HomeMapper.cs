using Overoom.Application.Abstractions.StartPage.DTOs;
using Overoom.WEB.Mappers.Abstractions;
using Overoom.WEB.Models.Home;

namespace Overoom.WEB.Mappers;

public class HomeMapper : IHomeMapper
{
    public CommentStartPageViewModel Map(CommentDto dto) =>
        new(dto.Name, dto.Text, dto.DateTime, dto.FilmId, dto.AvatarUri);

    public RoomStartPageViewModel Map(RoomDto dto) => new(dto.Id, dto.Type, dto.CountUsers, dto.NowPlaying);

    public FilmStartPageViewModel Map(FilmDto dto) => new(dto.Name, dto.PosterUrl, dto.Id, dto.Genres);
}