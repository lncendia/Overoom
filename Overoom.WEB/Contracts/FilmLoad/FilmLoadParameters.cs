﻿using System.ComponentModel.DataAnnotations;
using Overoom.Domain.Films.Enums;

namespace Overoom.WEB.Contracts.FilmLoad;

public class FilmLoadParameters
{
    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [StringLength(1500, ErrorMessage = "Не больше 1500 символов")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Не больше 500 символов")]
    [Display(Name = "Короткое описание")]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Тип")]
    public FilmType Type { get; set; }

    [Display(Name = "Ссылка на постер")]
    [DataType(DataType.ImageUrl)]
    public string? PosterUri { get; set; }

    [Display(Name = "Постер")]
    [DataType(DataType.Upload)]
    public IFormFile? Poster { get; set; }

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [StringLength(200, ErrorMessage = "Не больше 200 символов")]
    [Display(Name = "Название")]
    public string Name { get; set; } = null!;


    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Год")]
    [Range(1800, 2100, ErrorMessage = "Введите корректный год выхода")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Рейтинг")]
    [Range(0d, 10d, ErrorMessage = "Рейтинг должен быть в диапазоне от 0 до 10")]
    public double Rating { get; set; }

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Количество сезонов")]
    public int? CountSeasons { get; set; }

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Количество серий")]
    public int? CountEpisodes { get; set; }

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Cdn")]
    public List<CdnParameters> Cdns { get; set; } = null!;

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Страны")]
    public List<TitleParameters> Countries { get; set; } = null!;

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Актёры")]
    public List<ActorParameters> Actors { get; set; } = null!;

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Режиссёры")]
    public List<PersonParameters> Directors { get; set; } = null!;

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Жанры")]
    public List<TitleParameters> Genres { get; set; } = null!;

    [Required(ErrorMessage = "Поле не должно быть пустым")]
    [Display(Name = "Сценаристы")]
    public List<PersonParameters> Screenwriters { get; set; } = null!;
}