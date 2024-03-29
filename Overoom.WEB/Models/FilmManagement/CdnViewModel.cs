using Overoom.Domain.Films.Enums;

namespace Overoom.WEB.Models.FilmManagement;

public class CdnViewModel
{
    public CdnViewModel(CdnType type, Uri uri, string quality, IReadOnlyCollection<string> voices)
    {
        Type = type.ToString();
        Uri = uri;
        Quality = quality;
        Voices = voices;
    }

    public string Type { get; }
    public Uri Uri { get; }

    public string Quality { get; }

    public IReadOnlyCollection<string> Voices { get; }
}