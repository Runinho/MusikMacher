using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusikMacher;
using MusikMacher.Migrations;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LorusMusikMacher.database
{

  public class TrackContext : DbContext
  {
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public string DbPath { get; }

    // used for design time
    public TrackContext(): this("designTrack")
    { 

    }

      public TrackContext(string name)
    {
      var folder = Environment.SpecialFolder.LocalApplicationData;
      var path = Environment.GetFolderPath(folder);
      DbPath = System.IO.Path.Join(path, $"{name}.db");
      Console.WriteLine($"Using db at: {DbPath}");
    }

    public static TrackContext GetTrackContext(string name)
    {
      var db = new TrackContext(name);
      var migrator = db.GetInfrastructure().GetService<IMigrator>();

      // get applied migrations.
      List<string> applyedMigrations = db.Database.GetAppliedMigrations().ToList();
      foreach (var migrationName in db.Database.GetPendingMigrations())
      {
        Console.WriteLine($"doing migration: {migrationName}");
        if (migrationName == "20240504101556_InitialCreate")
        {
          // we try to do it once and then we try to disable it LOL
          try
          {
            migrator.Migrate(migrationName);
          } catch (SqliteException ex)
          {
            Console.WriteLine($"migration falied with exception: {ex.Message}");
            if (ex.Message == "SQLite Error 1: 'table \"Tags\" already exists'.")
            {
              Console.WriteLine("trying with disabled migration...");
              InitialCreate.tryCreation = false;
              migrator.Migrate(migrationName);
              InitialCreate.tryCreation = true; // for next database
            }
          }
        }else
        {
          // just do it.
          migrator.Migrate(migrationName);
        }
      }

        //db.Database.Migrate();
        db.Database.OpenConnection();
      //db.Database.EnsureCreated();
      return db;
    }  

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
      options.UseSqlite($"Data Source={DbPath}");

      //options.LogTo(message => System.Diagnostics.Debug.WriteLine(message), LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Track>()
          .HasMany(p => p.Tags)
          .WithMany();
    }

    public Tag CreateOrFindTag(string name)
    {
      var tag = Tags.Where(t => t.Name == name)
                    .FirstOrDefault();
      if (tag == null)
      {
        tag = new Tag { Name = name };
        Tags.Add(tag);

        return tag;
      }
      return tag;
    }

  }

}
