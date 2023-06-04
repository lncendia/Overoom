using Overoom.Domain.Film.DTOs;
using Overoom.Domain.Film.Enums;

namespace Overoom.Domain.Film.Entities;

public class FilmBuilder
{
    private FilmBuilder()
    {
    }

    private string? _name;
    private string? _description;
    private string? _shortDescription;
    private DateOnly? _date;
    private double? _rating;
    private FilmType? _type;
    private IEnumerable<CdnDto>? _cdnList;
    private IEnumerable<string>? _genres;
    private IEnumerable<(string name, string description)>? _actors;
    private IEnumerable<string>? _directors;
    private IEnumerable<string>? _screenwriters;
    private IEnumerable<string>? _countries;
    private string? _posterFileName;
    private int? _countSeasons;
    private int? _countEpisodes;

    public FilmBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public FilmBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public FilmBuilder WithShortDescription(string description)
    {
        _shortDescription = description;
        return this;
    }

    public FilmBuilder WithDate(DateOnly date)
    {
        _date = date;
        return this;
    }

    public FilmBuilder WithRating(double rating)
    {
        _rating = rating;
        return this;
    }

    public FilmBuilder WithType(FilmType type)
    {
        _type = type;
        return this;
    }

    public FilmBuilder WithCdn(IEnumerable<CdnDto> cdn)
    {
        _cdnList = cdn;
        return this;
    }

    public FilmBuilder WithGenres(IEnumerable<string> genres)
    {
        _genres = genres;
        return this;
    }

    public FilmBuilder WithActors(IEnumerable<(string name, string description)> actors)
    {
        _actors = actors;
        return this;
    }

    public FilmBuilder WithDirectors(IEnumerable<string> directors)
    {
        _directors = directors;
        return this;
    }

    public FilmBuilder WithScreenwriters(IEnumerable<string> screenwriters)
    {
        _screenwriters = screenwriters;
        return this;
    }

    public FilmBuilder WithCountries(IEnumerable<string> countries)
    {
        _countries = countries;
        return this;
    }

    public FilmBuilder WithPoster(string patch)
    {
        _posterFileName = patch;
        return this;
    }

    public FilmBuilder WithEpisodes(int countSeasons, int countEpisodes)
    {
        _countSeasons = countSeasons;
        _countEpisodes = countEpisodes;
        return this;
    }

    public Film Build()
    {
        if (string.IsNullOrEmpty(_name)) throw new InvalidOperationException("builder not formed");
        if (string.IsNullOrEmpty(_description)) throw new InvalidOperationException("builder not formed");
        if (_date == null) throw new InvalidOperationException("builder not formed");
        if (_rating == null) throw new InvalidOperationException("builder not formed");
        if (_type == null) throw new InvalidOperationException("builder not formed");
        if (_cdnList == null) throw new InvalidOperationException("builder not formed");
        if (_posterFileName == null) throw new InvalidOperationException("builder not formed");
        if (_genres == null) throw new InvalidOperationException("builder not formed");
        if (_actors == null) throw new InvalidOperationException("builder not formed");
        if (_directors == null) throw new InvalidOperationException("builder not formed");
        if (_screenwriters == null) throw new InvalidOperationException("builder not formed");
        if (_countries == null) throw new InvalidOperationException("builder not formed");

        return new Film(_name, _description, _shortDescription, _date.Value, _rating.Value, _type.Value, _cdnList,
            _genres, _actors, _directors, _screenwriters, _countries, _posterFileName, _countSeasons, _countEpisodes);
    }
}