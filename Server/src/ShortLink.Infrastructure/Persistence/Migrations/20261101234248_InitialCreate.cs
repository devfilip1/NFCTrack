using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortLink.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Links",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Slug = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                DestinationUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Links", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Links_Slug",
            table: "Links",
            column: "Slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Links");
    }
}
