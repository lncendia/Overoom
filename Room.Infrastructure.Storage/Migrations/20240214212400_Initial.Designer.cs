﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Room.Infrastructure.Storage.Context;

#nullable disable

namespace Room.Infrastructure.Storage.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240214212400_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Film.CdnModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<Guid>("FilmId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FilmId");

                    b.ToTable("FilmCdns");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Film.FilmModel", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1500)
                        .HasColumnType("character varying(1500)");

                    b.Property<string>("PosterUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("Year")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Films");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.BannedModel<Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("FilmRoomsBannedUsers");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.BannedModel<Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("YoutubeRoomsBannedUsers");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.MessageModel<Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.HasKey("UserId", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("FilmViewersMessages");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.MessageModel<Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.HasKey("UserId", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("YoutubeViewersMessages");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("CdnName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Code")
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)");

                    b.Property<Guid>("FilmId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("FilmId");

                    b.ToTable("FilmRooms");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmViewerModel", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.Property<string>("CustomName")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<bool>("FullScreen")
                        .HasColumnType("boolean");

                    b.Property<bool>("Online")
                        .HasColumnType("boolean");

                    b.Property<bool>("Owner")
                        .HasColumnType("boolean");

                    b.Property<bool>("Pause")
                        .HasColumnType("boolean");

                    b.Property<int?>("Season")
                        .HasColumnType("integer");

                    b.Property<int?>("Series")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("TimeLine")
                        .HasColumnType("interval");

                    b.HasKey("UserId", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("FilmViewers");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.VideoModel", b =>
                {
                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.Property<string>("VideoId")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime>("Added")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("RoomId", "VideoId");

                    b.ToTable("YoutubeRoomsVideoIds");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<bool>("Access")
                        .HasColumnType("boolean");

                    b.Property<string>("Code")
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)");

                    b.Property<DateTime>("LastActivity")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("YoutubeRooms");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeViewerModel", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.Property<string>("CustomName")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<bool>("FullScreen")
                        .HasColumnType("boolean");

                    b.Property<bool>("Online")
                        .HasColumnType("boolean");

                    b.Property<bool>("Owner")
                        .HasColumnType("boolean");

                    b.Property<bool>("Pause")
                        .HasColumnType("boolean");

                    b.Property<TimeSpan>("TimeLine")
                        .HasColumnType("interval");

                    b.Property<string>("VideoId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("UserId", "RoomId");

                    b.HasIndex("RoomId");

                    b.ToTable("YoutubeViewers");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.User.UserModel", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<bool>("Beep")
                        .HasColumnType("boolean");

                    b.Property<bool>("Change")
                        .HasColumnType("boolean");

                    b.Property<string>("PhotoUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Scream")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Film.CdnModel", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Film.FilmModel", "Film")
                        .WithMany("CdnList")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Film");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.BannedModel<Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel>", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel", "Room")
                        .WithMany("BannedUsers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Room.Infrastructure.Storage.Models.User.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.BannedModel<Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel>", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel", "Room")
                        .WithMany("BannedUsers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Room.Infrastructure.Storage.Models.User.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.MessageModel<Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel>", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel", "Room")
                        .WithMany("Messages")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Room.Infrastructure.Storage.Models.User.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.Base.MessageModel<Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel>", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel", "Room")
                        .WithMany("Messages")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Room.Infrastructure.Storage.Models.User.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Film.FilmModel", "Film")
                        .WithMany()
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Film");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmViewerModel", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel", "Room")
                        .WithMany("Viewers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Room.Infrastructure.Storage.Models.User.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.VideoModel", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel", "Room")
                        .WithMany("Videos")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeViewerModel", b =>
                {
                    b.HasOne("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel", "Room")
                        .WithMany("Viewers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Room.Infrastructure.Storage.Models.User.UserModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Film.FilmModel", b =>
                {
                    b.Navigation("CdnList");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.FilmRoom.FilmRoomModel", b =>
                {
                    b.Navigation("BannedUsers");

                    b.Navigation("Messages");

                    b.Navigation("Viewers");
                });

            modelBuilder.Entity("Room.Infrastructure.Storage.Models.Room.YoutubeRoom.YoutubeRoomModel", b =>
                {
                    b.Navigation("BannedUsers");

                    b.Navigation("Messages");

                    b.Navigation("Videos");

                    b.Navigation("Viewers");
                });
#pragma warning restore 612, 618
        }
    }
}
