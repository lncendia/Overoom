namespace Films.Domain.Ratings.Snapshots;

public class RatingSnapshot
{
    public required Guid Id { get; init; }
    public required Guid FilmId { get; init; }
    public required Guid UserId { get; init; }
    public required double Score { get; init; }
    public required DateTime CreatedAt { get; init; }
}
