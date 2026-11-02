using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportTournaments.NotificationService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailDeliveryLease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "lock_token",
                table: "email_deliveries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "locked_until_utc",
                table: "email_deliveries",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lock_token",
                table: "email_deliveries");

            migrationBuilder.DropColumn(
                name: "locked_until_utc",
                table: "email_deliveries");
        }
    }
}
