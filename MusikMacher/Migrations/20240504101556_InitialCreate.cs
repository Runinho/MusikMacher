using System;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusikMacher.Migrations
{
  /// <inheritdoc />
  public partial class InitialCreate : Migration
  {
    public static bool tryCreation = true;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      if (tryCreation)
      {
        Console.WriteLine("Trying to create tables");
        migrationBuilder.CreateTable(
          name: "Tags",
          columns: table => new
          {
            Id = table.Column<int>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", nullable: false),
            IsChecked = table.Column<bool>(type: "INTEGER", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Tags", x => x.Id);
          });

        migrationBuilder.CreateTable(
            name: "Tracks",
            columns: table => new
            {
              name = table.Column<string>(type: "TEXT", nullable: false),
              path = table.Column<string>(type: "TEXT", nullable: false),
              length = table.Column<int>(type: "INTEGER", nullable: true),
              creationTime = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
              table.PrimaryKey("PK_Tracks", x => x.name);
            });

        migrationBuilder.CreateTable(
            name: "TagTrack",
            columns: table => new
            {
              TagsId = table.Column<int>(type: "INTEGER", nullable: false),
              Trackname = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
              table.PrimaryKey("PK_TagTrack", x => new { x.TagsId, x.Trackname });
              table.ForeignKey(
                        name: "FK_TagTrack_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
              table.ForeignKey(
                        name: "FK_TagTrack_Tracks_Trackname",
                        column: x => x.Trackname,
                        principalTable: "Tracks",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TagTrack_Trackname",
            table: "TagTrack",
            column: "Trackname");
      }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "TagTrack");

      migrationBuilder.DropTable(
          name: "Tags");

      migrationBuilder.DropTable(
          name: "Tracks");
    }
  }
}
