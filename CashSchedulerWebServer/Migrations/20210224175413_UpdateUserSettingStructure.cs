using Microsoft.EntityFrameworkCore.Migrations;

namespace CashSchedulerWebServer.Migrations
{
    public partial class UpdateUserSettingStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_Users_UserId",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "UserSettings");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserSettings",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettingName",
                table: "UserSettings",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Label = table.Column<string>(nullable: false),
                    UnitName = table.Column<string>(nullable: false),
                    SectionName = table.Column<string>(nullable: false),
                    ValueType = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_SettingName",
                table: "UserSettings",
                column: "SettingName");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_Settings_SettingName",
                table: "UserSettings",
                column: "SettingName",
                principalTable: "Settings",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_UserSettings_Settings_SettingName",
                table: "UserSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_Users_UserId",
                table: "UserSettings");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_UserSettings_SettingName",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "SettingName",
                table: "UserSettings");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserSettings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_Users_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
