// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Overoom.Infrastructure.PersistentStorage.Context;

#nullable disable

namespace Overoom.Infrastructure.PersistentStorage.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.8");

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Comments.CommentModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FilmId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.ActorModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FilmModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FilmModelId");

                    b.ToTable("Actors");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.CountryModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("FilmModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FilmModelId");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.DirectorModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("FilmModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FilmModelId");

                    b.ToTable("Directors");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.FilmModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int?>("CountEpisodes")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CountSeasons")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PosterFileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Rating")
                        .HasColumnType("REAL");

                    b.Property<string>("ShortDescription")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Films");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.GenreModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("FilmModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FilmModelId");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.ScreenWriterModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("FilmModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FilmModelId");

                    b.ToTable("ScreenWriters");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Playlists.PlaylistModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FilmsList")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PosterFileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.RoomBaseModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsOpen")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RoomBaseModel");

                    b.HasDiscriminator<string>("Discriminator").HasValue("RoomBaseModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.ViewerBaseModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AvatarFileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("OnPause")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Online")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("TimeLine")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ViewerBaseModel");

                    b.HasDiscriminator<string>("Discriminator").HasValue("ViewerBaseModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.MessageModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ViewerId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.HasIndex("ViewerId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.VideoIdModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.ToTable("VideoIds");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Users.UserModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AvatarFileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FavoriteFilmsList")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("WatchedFilmsList")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.FilmRoomModel", b =>
                {
                    b.HasBaseType("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.RoomBaseModel");

                    b.Property<Guid>("FilmId")
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue("FilmRoomModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.FilmViewerModel", b =>
                {
                    b.HasBaseType("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.ViewerBaseModel");

                    b.Property<int>("Season")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Series")
                        .HasColumnType("INTEGER");

                    b.HasIndex("RoomId");

                    b.HasDiscriminator().HasValue("FilmViewerModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.YoutubeRoomModel", b =>
                {
                    b.HasBaseType("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.RoomBaseModel");

                    b.Property<bool>("AddAccess")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue("YoutubeRoomModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.YoutubeViewerModel", b =>
                {
                    b.HasBaseType("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.ViewerBaseModel");

                    b.Property<string>("CurrentVideoId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasIndex("RoomId");

                    b.HasDiscriminator().HasValue("YoutubeViewerModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.ActorModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Films.FilmModel", "FilmModel")
                        .WithMany("Actors")
                        .HasForeignKey("FilmModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FilmModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.CountryModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Films.FilmModel", "FilmModel")
                        .WithMany("Countries")
                        .HasForeignKey("FilmModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FilmModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.DirectorModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Films.FilmModel", "FilmModel")
                        .WithMany("Directors")
                        .HasForeignKey("FilmModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FilmModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.GenreModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Films.FilmModel", "FilmModel")
                        .WithMany("Genres")
                        .HasForeignKey("FilmModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FilmModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.ScreenWriterModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Films.FilmModel", "FilmModel")
                        .WithMany("ScreenWriters")
                        .HasForeignKey("FilmModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FilmModel");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.MessageModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.RoomBaseModel", "Room")
                        .WithMany("Messages")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.ViewerBaseModel", "Viewer")
                        .WithMany()
                        .HasForeignKey("ViewerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("Viewer");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.VideoIdModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Rooms.YoutubeRoomModel", "Room")
                        .WithMany("VideoIds")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.FilmViewerModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Rooms.FilmRoomModel", "Room")
                        .WithMany("Viewers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.YoutubeViewerModel", b =>
                {
                    b.HasOne("Overoom.Infrastructure.PersistentStorage.Models.Rooms.YoutubeRoomModel", "Room")
                        .WithMany("Viewers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Films.FilmModel", b =>
                {
                    b.Navigation("Actors");

                    b.Navigation("Countries");

                    b.Navigation("Directors");

                    b.Navigation("Genres");

                    b.Navigation("ScreenWriters");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.Base.RoomBaseModel", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.FilmRoomModel", b =>
                {
                    b.Navigation("Viewers");
                });

            modelBuilder.Entity("Overoom.Infrastructure.PersistentStorage.Models.Rooms.YoutubeRoomModel", b =>
                {
                    b.Navigation("VideoIds");

                    b.Navigation("Viewers");
                });
#pragma warning restore 612, 618
        }
    }
}
