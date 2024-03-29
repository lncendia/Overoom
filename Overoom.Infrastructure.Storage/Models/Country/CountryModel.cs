﻿namespace Overoom.Infrastructure.Storage.Models.Country;

public class CountryModel
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameNormalized { get; set; } = null!;
}