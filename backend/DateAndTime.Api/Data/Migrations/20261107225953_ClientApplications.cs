using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateAndTime.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClientApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "applications",
                type: "char(36)",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci").Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `users` (`Id` char(36) NOT NULL, `Email` varchar(254) NOT NULL, `DisplayName` varchar(120) NOT NULL, `PasswordHash` varchar(300) NOT NULL, `Role` varchar(32) NOT NULL, `IsActive` tinyint(1) NOT NULL, `CreatedAtUtc` datetime(6) NOT NULL, CONSTRAINT `PK_users` PRIMARY KEY (`Id`), CONSTRAINT `IX_users_Email` UNIQUE (`Email`)) CHARACTER SET utf8mb4");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `portfolio_items` (`Id` char(36) NOT NULL, `Title` varchar(160) NOT NULL, `Type` varchar(120) NOT NULL, `Description` varchar(2000) NOT NULL, `ImageUrl` varchar(500) NULL, `DisplayOrder` int NOT NULL, `IsPublished` tinyint(1) NOT NULL, CONSTRAINT `PK_portfolio_items` PRIMARY KEY (`Id`)) CHARACTER SET utf8mb4");
            migrationBuilder.Sql("CREATE TABLE IF NOT EXISTS `event_types` (`Id` char(36) NOT NULL, `Code` varchar(32) NOT NULL, `Name` varchar(120) NOT NULL, `IsActive` tinyint(1) NOT NULL, CONSTRAINT `PK_event_types` PRIMARY KEY (`Id`), CONSTRAINT `IX_event_types_Code` UNIQUE (`Code`)) CHARACTER SET utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_applications_UserId",
                table: "applications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_applications_users_UserId",
                table: "applications",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_applications_users_UserId",
                table: "applications");

            migrationBuilder.DropIndex(
                name: "IX_applications_UserId",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "applications");
        }
    }
}
