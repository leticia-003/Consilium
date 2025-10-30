namespace Consilium.Domain.Enums
{
    // Maps to Postgres ENUM: log_operation_process
    public enum LogOperationProcess
    {
        CREATE,
        UPDATE,
        CLOSE,
        REOPEN,
        DELETE
    }
}
