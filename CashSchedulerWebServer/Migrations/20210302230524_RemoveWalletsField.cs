using Microsoft.EntityFrameworkCore.Migrations;

namespace CashSchedulerWebServer.Migrations
{
    public partial class RemoveWalletsField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "Wallets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "Wallets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
