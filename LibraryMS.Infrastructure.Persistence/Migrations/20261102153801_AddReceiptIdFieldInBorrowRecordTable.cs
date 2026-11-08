using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptIdFieldInBorrowRecordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptId",
                table: "BorrowRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_ReceiptId",
                table: "BorrowRecords",
                column: "ReceiptId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BorrowRecords_ReceiptId",
                table: "BorrowRecords");

            migrationBuilder.DropColumn(
                name: "ReceiptId",
                table: "BorrowRecords");
        }
    }
}
