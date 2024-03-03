namespace SampleApiLoggerSerilog.Setup.LoggerSetup;

public interface ILoggerApplication<T>
{
    void InfoApp(string message);
    void WarnApp(string message);
    void ErrorApp(string message, Exception ex);

    void InfoAudit(string message, string personName, string description);
    void WarnAudit(string message, string personName, string description);
    void ErrorAudit(string message, string personName, string description, Exception ex);
}
