namespace Films.Application.Abstractions.Services.MovieApi.DTOs;

public class ActorApiResponse
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}