using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noelle.Todo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditingFieldsType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "last_modified_by",
                table: "todo_items",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "最后修改人",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "最后修改人");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "todo_items",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "创建人",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "创建人");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictTokens",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "last_modified_by",
                table: "todo_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "最后修改人",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "最后修改人");

            migrationBuilder.AlterColumn<long>(
                name: "created_by",
                table: "todo_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "创建人",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "创建人");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OpenIddictTokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);
        }
    }
}
