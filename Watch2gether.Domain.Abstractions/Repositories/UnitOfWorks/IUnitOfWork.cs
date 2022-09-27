namespace Watch2gether.Domain.Abstractions.Repositories.UnitOfWorks;

public interface IUnitOfWork
{
    Lazy<IFilmRepository> FilmRepository { get; }
    Lazy<IUserRepository> UserRepository { get; }
    Lazy<IFilmRoomRepository> FilmRoomRepository { get; }
    Lazy<IYoutubeRoomRepository> YoutubeRoomRepository { get; }
    Lazy<IPlaylistRepository> PlaylistRepository { get; } 
    Lazy<ICommentRepository> CommentRepository { get; } 
    Task SaveAsync();
}