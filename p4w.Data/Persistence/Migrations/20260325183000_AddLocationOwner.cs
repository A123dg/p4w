using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace p4w.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Location",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_OwnerId",
                table: "Location",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Location_User_OwnerId",
                table: "Location",
                column: "OwnerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_User_OwnerId",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_OwnerId",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Location");
        }
    }
}
