using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Noelle.Todo.Domain.Todo.Entities;

namespace Noelle.Todo.Infrastructure.EntityConfigurations;

internal class TodoItemEntityTypeConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("todo_items");
        builder.Property(s => s.Id).HasColumnName("id").HasComment("主键");
        builder.Property(s => s.Name).IsRequired().IsUnicode().HasColumnName("name").HasComment("待办事项名称");
        builder.Property(s => s.IsComplete).HasColumnName("is_complete").HasComment("待办事项是否已完成");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasComment("创建时间");
        builder.Property(s => s.CreatedBy).HasColumnName("created_by").HasComment("创建人");
        builder.Property(s => s.LastModifiedAt).HasColumnName("last_modified_at").HasComment("最后修改时间");
        builder.Property(s => s.LastModifiedBy).HasColumnName("last_modified_by").HasComment("最后修改人");
    }
}
