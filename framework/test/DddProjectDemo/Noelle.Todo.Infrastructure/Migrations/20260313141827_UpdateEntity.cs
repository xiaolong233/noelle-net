using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noelle.Todo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "todo_items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                comment: "事项名称",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "待办事项名称");

            migrationBuilder.AlterColumn<string>(
                name: "last_modified_by",
                table: "todo_items",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true,
                comment: "最后修改人",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "最后修改人");

            migrationBuilder.AlterColumn<bool>(
                name: "is_complete",
                table: "todo_items",
                type: "bit",
                nullable: false,
                comment: "是否已完成",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "待办事项是否已完成");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "todo_items",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true,
                comment: "创建人",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "创建人");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "todo_items",
                type: "nvarchar(max)",
                nullable: false,
                comment: "待办事项名称",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldComment: "事项名称");

            migrationBuilder.AlterColumn<string>(
                name: "last_modified_by",
                table: "todo_items",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "最后修改人",
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36,
                oldNullable: true,
                oldComment: "最后修改人");

            migrationBuilder.AlterColumn<bool>(
                name: "is_complete",
                table: "todo_items",
                type: "bit",
                nullable: false,
                comment: "待办事项是否已完成",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "是否已完成");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "todo_items",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "创建人",
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36,
                oldNullable: true,
                oldComment: "创建人");
        }
    }
}
