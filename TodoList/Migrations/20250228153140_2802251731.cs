using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoList.Migrations
{
    /// <inheritdoc />
    public partial class _2802251731 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todo_Projects_ProjectId",
                table: "Todo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Todo",
                table: "Todo");

            migrationBuilder.RenameTable(
                name: "Todo",
                newName: "Todos");

            migrationBuilder.RenameIndex(
                name: "IX_Todo_ProjectId",
                table: "Todos",
                newName: "IX_Todos_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Todos",
                table: "Todos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Projects_ProjectId",
                table: "Todos",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Projects_ProjectId",
                table: "Todos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Todos",
                table: "Todos");

            migrationBuilder.RenameTable(
                name: "Todos",
                newName: "Todo");

            migrationBuilder.RenameIndex(
                name: "IX_Todos_ProjectId",
                table: "Todo",
                newName: "IX_Todo_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Todo",
                table: "Todo",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Todo_Projects_ProjectId",
                table: "Todo",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
