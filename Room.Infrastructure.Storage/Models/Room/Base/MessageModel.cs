using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Room.Infrastructure.Storage.Models.User;

namespace Room.Infrastructure.Storage.Models.Room.Base;

[PrimaryKey(nameof(UserId), nameof(RoomId))]
public class MessageModel<TR> where TR : RoomModel
{
    public Guid UserId { get; set; }

    public Guid RoomId { get; set; }
    
    public UserModel User { get; set; } = null!;
    public TR Room { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    [MaxLength(1000)] public string Text { get; set; } = null!;
}