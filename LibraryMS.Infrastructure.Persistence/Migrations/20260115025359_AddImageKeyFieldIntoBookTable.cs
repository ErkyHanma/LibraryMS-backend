using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImageKeyFieldIntoBookTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverUrl",
                table: "Books",
                newName: "CoverImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageKey",
                table: "Books",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageKey",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "CoverImageUrl",
                table: "Books",
                newName: "CoverUrl");
        }
    }
}
