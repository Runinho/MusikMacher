using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusikMacher;
using System;
using System.Collections.Generic;

namespace LorusMusikMacher.database
{

  public class TrackContext : DbContext
  {
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public string DbPath { get; }

    public TrackContext()
    {
      var folder = Environment.SpecialFolder.LocalApplicationData;
      var path = Environment.GetFolderPath(folder);
      DbPath = System.IO.Path.Join(path, "tracks.db");
      Console.WriteLine($"Using db at: {DbPath}");
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
