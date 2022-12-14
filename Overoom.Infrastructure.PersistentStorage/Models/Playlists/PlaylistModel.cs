using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Overoom.Infrastructure.PersistentStorage.Models.Playlists;

public class PlaylistModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string FilmsList { get; set; } = null!;
    public DateTime Updated { get; set; }
    public string PosterFileName { get; set; } = null!;

    [NotMapped]
    public List<Guid> Films
    {
        get => JsonSerializer.Deserialize<List<Guid>>(FilmsList, new JsonSerializerOptions
            {Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}) ?? new List<Guid>();
        set => FilmsList = JsonSerializer.Serialize(value, new JsonSerializerOptions
            {Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping});
    }
}