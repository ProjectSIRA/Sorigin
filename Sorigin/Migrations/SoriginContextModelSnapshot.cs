﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Sorigin;

namespace Sorigin.Migrations
{
    [DbContext(typeof(SoriginContext))]
    partial class SoriginContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Sorigin.Models.Platforms.DiscordUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("avatar");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("discriminator");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_discord_user");

                    b.ToTable("discord_user");
                });

            modelBuilder.Entity("Sorigin.Models.Platforms.SteamUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("avatar");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_steam_user");

                    b.ToTable("steam_user");
                });

            modelBuilder.Entity("Sorigin.Models.User", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Bio")
                        .HasColumnType("text")
                        .HasColumnName("bio");

                    b.Property<string>("DiscordId")
                        .HasColumnType("text")
                        .HasColumnName("discord_id");

                    b.Property<int>("GamePlatform")
                        .HasColumnType("integer")
                        .HasColumnName("game_platform");

                    b.Property<int>("Role")
                        .HasColumnType("integer")
                        .HasColumnName("role");

                    b.Property<string>("SteamId")
                        .HasColumnType("text")
                        .HasColumnName("steam_id");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("ID")
                        .HasName("pk_users");

                    b.HasIndex("DiscordId")
                        .HasDatabaseName("ix_users_discord_id");

                    b.HasIndex("SteamId")
                        .HasDatabaseName("ix_users_steam_id");

                    b.HasIndex("Username")
                        .HasDatabaseName("ix_users_username");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Sorigin.Models.User", b =>
                {
                    b.HasOne("Sorigin.Models.Platforms.DiscordUser", "Discord")
                        .WithMany()
                        .HasForeignKey("DiscordId")
                        .HasConstraintName("fk_users_discord_user_discord_id");

                    b.HasOne("Sorigin.Models.Platforms.SteamUser", "Steam")
                        .WithMany()
                        .HasForeignKey("SteamId")
                        .HasConstraintName("fk_users_steam_user_steam_id");

                    b.Navigation("Discord");

                    b.Navigation("Steam");
                });
#pragma warning restore 612, 618
        }
    }
}