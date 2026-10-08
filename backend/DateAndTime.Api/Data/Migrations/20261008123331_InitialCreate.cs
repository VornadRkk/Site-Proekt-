using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DateAndTime.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClientName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PriceFrom = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "services",
                columns: new[] { "Id", "Category", "DisplayOrder", "IsActive", "Name", "PriceFrom", "ShortDescription" },
                values: new object[,]
                {
                    { new Guid("15373850-0b42-4e3f-87aa-fb2f6477cc31"), "Theatre", 1, true, "Театрализованная программа", 60000m, "Авторский сценарий, образы и живое представление для вашей площадки и зрителей." },
                    { new Guid("56ddf997-591d-419e-9c36-5acfdde51ce1"), "Corporate", 6, true, "Корпоративный праздник", 100000m, "Тематическая программа и представление для команды." },
                    { new Guid("752c9775-4c91-4d52-ab09-8515fb41b642"), "Birthday", 4, true, "День рождения", 75000m, "Личный сценарий для семейного праздника или большой вечеринки." },
                    { new Guid("88a41a6d-f625-4a32-a3b5-0727c888e312"), "Theatre", 3, true, "Интерактивный спектакль", 45000m, "История, в которой зрители становятся участниками происходящего." },
                    { new Guid("b0ce73b4-a9a1-41f2-a7e3-43c2988696f3"), "Wedding", 5, true, "Свадебная программа", 120000m, "Творческие номера и тёплые детали для важного дня." },
                    { new Guid("c9ddb5a8-f5d0-4180-aadf-d9304c782e34"), "Celebration", 2, true, "Праздник под ключ", 80000m, "Идея, оформление, артисты и программа, собранные в цельный праздник." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_applications_CreatedAtUtc",
                table: "applications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_applications_Status",
                table: "applications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_services_DisplayOrder",
                table: "services",
                column: "DisplayOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropTable(
                name: "services");
        }
    }
}
