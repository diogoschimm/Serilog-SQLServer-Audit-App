namespace SampleApiLoggerSerilog.Setup.LoggerSetup;

public class LoggerApplication<T>(ILogger<T> logger) : ILoggerApplication<T>
{
    private readonly ILogger<T> _logger = logger;

    public void ErrorApp(string message, Exception ex)
    { 
        _logger.LogError(
            ex,
            "{Titulo}{CreatedBy}{CreatedDate}{TableType}{StackTrace}",
            message,
            "1",
            DateTime.UtcNow,
            LoggerExtensions.OPERATION_TABLE_TYPE,
            ex.ToString());
    }

    public void ErrorAudit(string message, string personName, string description, Exception ex)
    { 
        _logger.LogError(
            ex,
            "{Titulo}{CreatedBy}{CreatedDate}{PersonName}{Description}{TableType}{StackTrace}",
            message,
            "1",
            DateTime.UtcNow,
            personName,
            description,
            LoggerExtensions.AUDIT_TABLE_TYPE, 
            ex.ToString());
    }

    public void InfoApp(string message)
    { 
        _logger.LogInformation(
            "{Titulo}{CreatedBy}{CreatedDate}{TableType}",
            message,
            "1",
            DateTime.UtcNow,
            LoggerExtensions.OPERATION_TABLE_TYPE);
    }

    public void InfoAudit(string message, string personName, string description)
    { 
        _logger.LogInformation(
            "{Titulo}{CreatedBy}{CreatedDate}{PersonName}{Description}{TableType}",
            message,
            "1",
            DateTime.UtcNow,
            personName,
            description,
            LoggerExtensions.AUDIT_TABLE_TYPE);
    }

    public void WarnApp(string message)
    {
        _logger.LogWarning(
            "{Titulo}{CreatedBy}{CreatedDate}{TableType}",
            message,
            "1",
            DateTime.UtcNow,
            LoggerExtensions.OPERATION_TABLE_TYPE);
    }

    public void WarnAudit(string message, string personName, string description)
    {
        _logger.LogWarning(
            "{Titulo}{CreatedBy}{CreatedDate}{PersonName}{Description}{TableType}",
            message,
            "1",
            DateTime.UtcNow,
            personName,
            description,
            LoggerExtensions.AUDIT_TABLE_TYPE);
    }
}

