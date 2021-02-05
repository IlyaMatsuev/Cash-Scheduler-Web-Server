using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CashSchedulerWebServer.Migrations
{
    public partial class AddWalletAndCurrencyEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Abbreviation = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IconUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Abbreviation);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyExchangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceCurrencyId = table.Column<string>(nullable: true),
                    TargetCurrencyId = table.Column<string>(nullable: true),
                    ExchangeRate = table.Column<float>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: false),
                    ValidTo = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    IsCustom = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyExchangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyExchangeRates_Currencies_SourceCurrencyId",
                        column: x => x.SourceCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Abbreviation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyExchangeRates_Currencies_TargetCurrencyId",
                        column: x => x.TargetCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Abbreviation",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyExchangeRates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Balance = table.Column<double>(nullable: false),
                    CurrencyAbbreviation1 = table.Column<string>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    IsCustom = table.Column<bool>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Currencies_CurrencyAbbreviation1",
                        column: x => x.CurrencyAbbreviation1,
                        principalTable: "Currencies",
                        principalColumn: "Abbreviation",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRates_SourceCurrencyId",
                table: "CurrencyExchangeRates",
                column: "SourceCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRates_TargetCurrencyId",
                table: "CurrencyExchangeRates",
                column: "TargetCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRates_UserId",
                table: "CurrencyExchangeRates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CurrencyAbbreviation1",
                table: "Wallets",
                column: "CurrencyAbbreviation1");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyExchangeRates");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
