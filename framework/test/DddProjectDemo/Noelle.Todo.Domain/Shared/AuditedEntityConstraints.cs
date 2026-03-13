namespace Noelle.Todo.Domain.Shared;

public static class AuditedEntityConstraints
{
    public static class CreatedBy
    {
        public const int MaxLength = 36;
    }

    public static class LastModifiedBy
    {
        public const int MaxLength = 36;
    }
}
