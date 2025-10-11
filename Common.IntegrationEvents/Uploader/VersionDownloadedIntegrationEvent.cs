namespace Common.IntegrationEvents.Uploader;

public class VersionDownloadedIntegrationEvent
{
    public required Guid FilmId { get; init; }
    public required string Version { get; init; }
    public int? Season { get; init; }
    public int? Episode { get; init; }
}