using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using p4w.Data.Persistence;

#nullable disable

namespace p4w.Data.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260326091500_RepairLocationOwnerColumn")]
    public partial class RepairLocationOwnerColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Location', 'OwnerId') IS NULL
                BEGIN
                    ALTER TABLE [Location] ADD [OwnerId] uniqueidentifier NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Location_OwnerId'
                      AND object_id = OBJECT_ID(N'[Location]')
                )
                BEGIN
                    CREATE INDEX [IX_Location_OwnerId] ON [Location] ([OwnerId]);
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_Location_User_OwnerId'
                )
                BEGIN
                    ALTER TABLE [Location]
                    ADD CONSTRAINT [FK_Location_User_OwnerId]
                    FOREIGN KEY ([OwnerId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_Location_User_OwnerId'
                )
                BEGIN
                    ALTER TABLE [Location] DROP CONSTRAINT [FK_Location_User_OwnerId];
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Location_OwnerId'
                      AND object_id = OBJECT_ID(N'[Location]')
                )
                BEGIN
                    DROP INDEX [IX_Location_OwnerId] ON [Location];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('Location', 'OwnerId') IS NOT NULL
                BEGIN
                    ALTER TABLE [Location] DROP COLUMN [OwnerId];
                END
                """);
        }
    }
}
