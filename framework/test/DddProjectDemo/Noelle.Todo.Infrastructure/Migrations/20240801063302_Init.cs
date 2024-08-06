using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noelle.Todo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "todo_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "主键"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "待办事项名称"),
                    is_complete = table.Column<bool>(type: "bit", nullable: false, comment: "待办事项是否已完成")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_items", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "todo_items");
        }
    }
}
