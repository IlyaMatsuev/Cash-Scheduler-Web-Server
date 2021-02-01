using Microsoft.EntityFrameworkCore.Migrations;

namespace CashSchedulerWebServer.Migrations
{
    public partial class RenameUserFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_TransactionTypes_TypeName",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_RegularTransactions_Users_CreatedById",
                table: "RegularTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RegularTransactions_Categories_TransactionCategoryId",
                table: "RegularTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_CreatedById",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_TransactionCategoryId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Users_CreatedForId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_CreatedForId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CreatedById",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionCategoryId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_RegularTransactions_CreatedById",
                table: "RegularTransactions");

            migrationBuilder.DropIndex(
                name: "IX_RegularTransactions_TransactionCategoryId",
                table: "RegularTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TypeName",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedForId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionCategoryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "RegularTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionCategoryId",
                table: "RegularTransactions");

            migrationBuilder.DropColumn(
                name: "TypeName",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserNotifications",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId1",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId1",
                table: "RegularTransactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "RegularTransactions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeName1",
                table: "Categories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId1",
                table: "Transactions",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularTransactions_CategoryId1",
                table: "RegularTransactions",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_RegularTransactions_UserId",
                table: "RegularTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TypeName1",
                table: "Categories",
                column: "TypeName1");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_TransactionTypes_TypeName1",
                table: "Categories",
                column: "TypeName1",
                principalTable: "TransactionTypes",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegularTransactions_Categories_CategoryId1",
                table: "RegularTransactions",
                column: "CategoryId1",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegularTransactions_Users_UserId",
                table: "RegularTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_CategoryId1",
                table: "Transactions",
                column: "CategoryId1",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Users_UserId",
                table: "UserNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_TransactionTypes_TypeName1",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_RegularTransactions_Categories_CategoryId1",
                table: "RegularTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RegularTransactions_Users_UserId",
                table: "RegularTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId1",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Users_UserId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CategoryId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_RegularTransactions_CategoryId1",
                table: "RegularTransactions");

            migrationBuilder.DropIndex(
                name: "IX_RegularTransactions_UserId",
                table: "RegularTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TypeName1",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "RegularTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RegularTransactions");

            migrationBuilder.DropColumn(
                name: "TypeName1",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "CreatedForId",
                table: "UserNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransactionCategoryId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "RegularTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransactionCategoryId",
                table: "RegularTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeName",
                table: "Categories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_CreatedForId",
                table: "UserNotifications",
                column: "CreatedForId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedById",
                table: "Transactions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionCategoryId",
                table: "Transactions",
                column: "TransactionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RegularTransactions_CreatedById",
                table: "RegularTransactions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RegularTransactions_TransactionCategoryId",
                table: "RegularTransactions",
                column: "TransactionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TypeName",
                table: "Categories",
                column: "TypeName");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_TransactionTypes_TypeName",
                table: "Categories",
                column: "TypeName",
                principalTable: "TransactionTypes",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegularTransactions_Users_CreatedById",
                table: "RegularTransactions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegularTransactions_Categories_TransactionCategoryId",
                table: "RegularTransactions",
                column: "TransactionCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_CreatedById",
                table: "Transactions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_TransactionCategoryId",
                table: "Transactions",
                column: "TransactionCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Users_CreatedForId",
                table: "UserNotifications",
                column: "CreatedForId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
