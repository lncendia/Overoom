using Overoom.Domain.Films.Enums;

namespace Overoom.Application.Abstractions.DTO.Films.FilmInfoGetter;

public class FilmInfoDto
{
    public FilmInfoDto(string id, string name, int year, FilmType type)
    {
        Name = name;
        Year = year;
        Type = type;
        Id = id;
    }

    public string Id { get; }
    public FilmType Type { get; }
    public string Name { get; }
    public int Year { get; }
}