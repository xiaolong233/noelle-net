using Noelle.Todo.Domain.Shared;
using Noelle.Todo.Domain.Todo;

namespace Noelle.Todo.Infrastructure.EntityConfigurations;

/// <summary>
/// <see cref="TodoItem"/> 的实体类型配置
/// </summary>
internal class TodoItemEntityTypeConfiguration : IEntityTypeConfiguration<TodoItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("todo_items");
        builder.HasKey(x => x.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .HasComment("主键");

        builder.Property(s => s.Name)
            .IsUnicode()
            .HasMaxLength(TodoItemConstraints.Name.MaxLength)
            .HasColumnName("name")
            .HasComment("事项名称");

        builder.Property(s => s.IsComplete)
            .HasColumnName("is_complete")
            .HasComment("是否已完成");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasComment("创建时间");

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(AuditedEntityConstraints.CreatedBy.MaxLength)
            .HasColumnName("created_by")
            .HasComment("创建人");

        builder.Property(s => s.LastModifiedAt)
            .HasColumnName("last_modified_at")
            .HasComment("最后修改时间");

        builder.Property(s => s.LastModifiedBy)
            .HasMaxLength(AuditedEntityConstraints.LastModifiedBy.MaxLength)
            .HasColumnName("last_modified_by")
            .HasComment("最后修改人");
    }
}
