using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportTournaments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayersOnFieldPerTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "players_on_field_per_team",
                table: "tournaments",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE tournaments
                SET players_on_field_per_team = 11
                WHERE players_on_field_per_team IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "players_on_field_per_team",
                table: "tournaments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "players_on_field_per_team",
                table: "tournaments");
        }
    }
}
