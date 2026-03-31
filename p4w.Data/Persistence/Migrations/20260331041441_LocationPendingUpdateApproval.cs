using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace p4w.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LocationPendingUpdateApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPendingUpdate",
                table: "Location",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PendingAddress",
                table: "Location",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingAddressLink",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PendingClosingHours",
                table: "Location",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingDescription",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingLocationName",
                table: "Location",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PendingOpeningHours",
                table: "Location",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PendingType",
                table: "Location",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingUpdatedAt",
                table: "Location",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPendingUpdate",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingAddress",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingAddressLink",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingClosingHours",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingDescription",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingLocationName",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingOpeningHours",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingType",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PendingUpdatedAt",
                table: "Location");
        }
    }
}
