using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortLink.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Links",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Links_OwnerId",
                table: "Links",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Links_AspNetUsers_OwnerId",
                table: "Links",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Links_AspNetUsers_OwnerId",
                table: "Links");

            migrationBuilder.DropIndex(
                name: "IX_Links_OwnerId",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Links");
        }
    }
}
