﻿namespace Overoom.Infrastructure.Storage.Models.Film;

public class DirectorModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public FilmModel FilmModel { get; set; } = null!;
}