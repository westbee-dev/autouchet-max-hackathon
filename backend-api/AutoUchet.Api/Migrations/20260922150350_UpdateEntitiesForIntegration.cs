using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoUchet.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesForIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RemindAboutTax",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Receipts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RemindAboutTax",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Receipts");
        }
    }
}
