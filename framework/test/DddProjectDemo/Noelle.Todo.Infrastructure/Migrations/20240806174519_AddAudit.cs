using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noelle.Todo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "todo_items",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "创建时间");

            migrationBuilder.AddColumn<long>(
                name: "created_by",
                table: "todo_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "创建人");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified_at",
                table: "todo_items",
                type: "datetime2",
                nullable: true,
                comment: "最后修改时间");

            migrationBuilder.AddColumn<long>(
                name: "last_modified_by",
                table: "todo_items",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "最后修改人");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "todo_items");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "todo_items");

            migrationBuilder.DropColumn(
                name: "last_modified_at",
                table: "todo_items");

            migrationBuilder.DropColumn(
                name: "last_modified_by",
                table: "todo_items");
        }
    }
}
