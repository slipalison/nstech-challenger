using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NsTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStaticSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AvailableQuantity", "Name", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), 100, "Product 1", 10.0m },
                    { new Guid("00000000-0000-0000-0000-000000000002"), 50, "Product 2", 20.0m },
                    { new Guid("00000000-0000-0000-0000-000000000003"), 10, "Product 3", 30.0m }
                });
        }
    }
}
