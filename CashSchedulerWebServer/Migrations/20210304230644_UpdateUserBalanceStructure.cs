using Microsoft.EntityFrameworkCore.Migrations;

namespace CashSchedulerWebServer.Migrations
{
    public partial class UpdateUserBalanceStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "WalletId1",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WalletId1",
                table: "RegularTransactions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId1",
                table: "Transactions",
                column: "WalletId1");

            migrationBuilder.CreateIndex(
                name: "IX_RegularTransactions_WalletId1",
                table: "RegularTransactions",
                column: "WalletId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RegularTransactions_Wallets_WalletId1",
                table: "RegularTransactions",
                column: "WalletId1",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletId1",
                table: "Transactions",
                column: "WalletId1",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegularTransactions_Wallets_WalletId1",
                table: "RegularTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_WalletId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_RegularTransactions_WalletId1",
                table: "RegularTransactions");

            migrationBuilder.DropColumn(
                name: "WalletId1",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "WalletId1",
                table: "RegularTransactions");

            migrationBuilder.AddColumn<double>(
                name: "Balance",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
