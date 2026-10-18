using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateAndTime.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2ClientAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These tables existed before they were added to the EF model: older
            // versions created them in DatabaseInitializer. IF NOT EXISTS keeps
            // this migration safe for both existing installations and new databases.
            migrationBuilder.Sql(
                @"CREATE TABLE IF NOT EXISTS `event_types` (
                    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `Code` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                    `Name` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    CONSTRAINT `PK_event_types` PRIMARY KEY (`Id`),
                    CONSTRAINT `IX_event_types_Code` UNIQUE (`Code`)
                ) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(
                @"CREATE TABLE IF NOT EXISTS `portfolio_items` (
                    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `Title` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
                    `Type` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `Description` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
                    `ImageUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `DisplayOrder` int NOT NULL,
                    `IsPublished` tinyint(1) NOT NULL,
                    CONSTRAINT `PK_portfolio_items` PRIMARY KEY (`Id`),
                    INDEX `IX_portfolio_items_IsPublished_DisplayOrder` (`IsPublished`, `DisplayOrder`)
                ) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(
                @"CREATE TABLE IF NOT EXISTS `users` (
                    `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    `Email` varchar(254) CHARACTER SET utf8mb4 NOT NULL,
                    `DisplayName` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
                    `PasswordHash` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
                    `Role` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    `CreatedAtUtc` datetime(6) NOT NULL,
                    CONSTRAINT `PK_users` PRIMARY KEY (`Id`),
                    CONSTRAINT `IX_users_Email` UNIQUE (`Email`)
                ) CHARACTER SET=utf8mb4;");

            migrationBuilder.UpdateData(
                table: "services",
                keyColumn: "Id",
                keyValue: new Guid("752c9775-4c91-4d52-ab09-8515fb41b642"),
                columns: new[] { "Name", "ShortDescription" },
                values: new object[] { "Частные праздники", "Личный сценарий для семейного события, камерного вечера или большой вечеринки." });

            migrationBuilder.UpdateData(
                table: "services",
                keyColumn: "Id",
                keyValue: new Guid("88a41a6d-f625-4a32-a3b5-0727c888e312"),
                columns: new[] { "Category", "Name", "ShortDescription" },
                values: new object[] { "Celebration", "Игровые программы", "Живые игровые форматы для детей и взрослых с ведущими, персонажами и общей историей." });

            migrationBuilder.InsertData(
                table: "services",
                columns: new[] { "Id", "Category", "DisplayOrder", "IsActive", "Name", "PriceFrom", "ShortDescription" },
                values: new object[] { new Guid("ab32a0d5-7eb7-4207-9d9b-e3fc5f750dee"), "Festival", 7, true, "Фестивальные программы", 150000m, "Концепция, сценарий и творческое наполнение для городских и тематических фестивалей." });

            // DatabaseInitializer adds applications.UserId and its index after
            // migrations because MySQL lacks portable ADD COLUMN IF NOT EXISTS.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "services",
                keyColumn: "Id",
                keyValue: new Guid("ab32a0d5-7eb7-4207-9d9b-e3fc5f750dee"));

            migrationBuilder.UpdateData(
                table: "services",
                keyColumn: "Id",
                keyValue: new Guid("752c9775-4c91-4d52-ab09-8515fb41b642"),
                columns: new[] { "Name", "ShortDescription" },
                values: new object[] { "День рождения", "Личный сценарий для семейного праздника или большой вечеринки." });

            migrationBuilder.UpdateData(
                table: "services",
                keyColumn: "Id",
                keyValue: new Guid("88a41a6d-f625-4a32-a3b5-0727c888e312"),
                columns: new[] { "Category", "Name", "ShortDescription" },
                values: new object[] { "Theatre", "Интерактивный спектакль", "История, в которой зрители становятся участниками происходящего." });
        }
    }
}
