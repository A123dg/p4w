using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace p4w.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationPreviousStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreviousStatus",
                table: "Location",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "Location");
        }
    }
}
