using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace p4w.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmailUnique1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLink",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.DropColumn(
                name: "AddressLink",
                table: "Location");
        }
    }
}
