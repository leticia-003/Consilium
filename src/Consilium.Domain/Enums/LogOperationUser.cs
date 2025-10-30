namespace Consilium.Domain.Enums
{
    // Maps to Postgres ENUM: log_operation_user
    public enum LogOperationUser
    {
        CREATE,
        UPDATE,
        DELETE,
        ACTIVATE,
        INACTIVATE
    }
}
