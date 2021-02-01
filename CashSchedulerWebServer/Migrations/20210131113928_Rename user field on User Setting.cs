using Microsoft.EntityFrameworkCore.Migrations;

namespace CashSchedulerWebServer.Migrations
{
    public partial class RenameuserfieldonUserSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_Users_SettingForId",
                table: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_UserSettings_SettingForId",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "SettingForId",
                table: "UserSettings");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserSettings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_Users_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_Users_UserId",
                table: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserSettings");

            migrationBuilder.AddColumn<int>(
                name: "SettingForId",
                table: "UserSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_SettingForId",
                table: "UserSettings",
                column: "SettingForId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_Users_SettingForId",
                table: "UserSettings",
                column: "SettingForId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
