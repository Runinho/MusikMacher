using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusikMacher.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHidden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Tags",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Tags");
        }
    }
}
