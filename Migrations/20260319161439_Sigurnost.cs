using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPMMA.Migrations
{
    /// <inheritdoc />
    public partial class Sigurnost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_ApplicationUserId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ApplicationUserId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Orders",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ApplicationUserId",
                table: "Orders",
                newName: "IX_Orders_ClientId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Carts",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_ApplicationUserId",
                table: "Carts",
                newName: "IX_Carts_ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_ClientId",
                table: "Carts",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ClientId",
                table: "Orders",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_ClientId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ClientId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Orders",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ClientId",
                table: "Orders",
                newName: "IX_Orders_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Carts",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_ClientId",
                table: "Carts",
                newName: "IX_Carts_ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_ApplicationUserId",
                table: "Carts",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ApplicationUserId",
                table: "Orders",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
