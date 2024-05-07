﻿// <auto-generated />
using System;
using LorusMusikMacher.database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MusikMacher.Migrations
{
    [DbContext(typeof(TrackContext))]
    [Migration("20240504161038_AddTagFavorite")]
    partial class AddTagFavorite
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("LorusMusikMacher.database.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsChecked")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("MusikMacher.Track", b =>
                {
                    b.Property<string>("name")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("creationTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("length")
                        .HasColumnType("INTEGER");

                    b.Property<string>("path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("name");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("TagTrack", b =>
                {
                    b.Property<int>("TagsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Trackname")
                        .HasColumnType("TEXT");

                    b.HasKey("TagsId", "Trackname");

                    b.HasIndex("Trackname");

                    b.ToTable("TagTrack");
                });

            modelBuilder.Entity("TagTrack", b =>
                {
                    b.HasOne("LorusMusikMacher.database.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MusikMacher.Track", null)
                        .WithMany()
                        .HasForeignKey("Trackname")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
