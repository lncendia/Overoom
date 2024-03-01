namespace Films.Application.Abstractions.MovieApi.DTOs;

public class ActorApiResponse
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}