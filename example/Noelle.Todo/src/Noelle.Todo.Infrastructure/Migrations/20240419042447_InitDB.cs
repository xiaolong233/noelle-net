using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noelle.Todo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "todo_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, comment: "主键", collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "longtext", nullable: false, comment: "待办事项名称")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_complete = table.Column<bool>(type: "tinyint(1)", nullable: false, comment: "待办事项是否已完成")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_items", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "todo_items");
        }
    }
}
