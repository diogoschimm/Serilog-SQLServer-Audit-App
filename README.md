# Serilog-SQLServer-Audit-App

## Program 

Adição do Serilog no pipeline 

```c#
builder.AddSerilogApplication();
builder.Services.AddSingleton(typeof(ILoggerApplication<>), typeof(LoggerApplication<>));
```

## String de Conexão

Configuração da String de conexão para o Logger do Serilog

```json
  "ConnectionStrings": {
    "LoggerConnection": "Data Source=dbs.diogoschimm.io\\SQL2019;Initial Catalog=DbLoggerSerilogSample;Integrated Security=true;TrustServerCertificate=True"
  }
```

## Extensions 

Configuração do Logger para o Pipeline do AspNetCore

```c#
﻿using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Data;

namespace SampleApiLoggerSerilog.Setup.LoggerSetup;

public static class LoggerExtensions
{
    public const string OPERATION_TABLE_TYPE = "Operation";
    public const string AUDIT_TABLE_TYPE = "Audit";

    public static IHostApplicationBuilder AddSerilogApplication(this IHostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .ConfigureLoggerOperation(builder.Configuration)
            .ConfigureLoggerAudit(builder.Configuration)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);

        return builder;
    }

    private static LoggerConfiguration ConfigureLoggerOperation(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LoggerConnection");

        var columnOptions = new ColumnOptions();
        columnOptions.Id.ColumnName = "IdLogOperacao";

        // Removing all the default column
        columnOptions.Store.Remove(StandardColumn.Message);
        columnOptions.Store.Remove(StandardColumn.TimeStamp);
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);

        //columnOptions.Store.Remove(StandardColumn.Level);
        //columnOptions.Store.Remove(StandardColumn.Exception);
        //columnOptions.Store.Remove(StandardColumn.Properties);

        // Adding all the custom columns
        columnOptions.AdditionalColumns = [
            new SqlColumn { DataType = SqlDbType.VarChar, ColumnName = "Titulo", DataLength = 250, AllowNull = true},
            new SqlColumn { DataType = SqlDbType.VarChar, ColumnName = "CreatedBy", DataLength = 50, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.DateTime2, ColumnName = "CreatedDate", DataLength = 7, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.VarChar , ColumnName = "TableType", DataLength = 100, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.VarChar , ColumnName = "StackTrace", DataLength = -1, AllowNull = true },
        ];


        loggerConfiguration.WriteTo.Logger(c =>
            c.Filter.ByIncludingOnly(evt => evt.Properties.GetValueOrDefault("TableType")?.ToString()?.Contains(OPERATION_TABLE_TYPE) ?? false)
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions()
                {
                    TableName = "LogOperacao",
                    AutoCreateSqlTable = true
                },
                columnOptions: columnOptions)
            );

        return loggerConfiguration;
    }

    public static LoggerConfiguration ConfigureLoggerAudit(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LoggerConnection");

        var columnOptions = new ColumnOptions();
        columnOptions.Id.ColumnName = "IdLogAuditoria";

        // Removing all the default column
        columnOptions.Store.Remove(StandardColumn.Message);
        columnOptions.Store.Remove(StandardColumn.TimeStamp);
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);

        //columnOptions.Store.Remove(StandardColumn.Level);
        //columnOptions.Store.Remove(StandardColumn.Exception);
        //columnOptions.Store.Remove(StandardColumn.Properties);

        // Adding all the custom columns
        columnOptions.AdditionalColumns = [
            new SqlColumn { DataType = SqlDbType.VarChar, ColumnName = "Titulo", DataLength = 250, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.VarChar, ColumnName = "CreatedBy", DataLength = 50, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.DateTime2, ColumnName = "CreatedDate", DataLength = 7, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.VarChar, ColumnName = "PersonName", DataLength = 100, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.VarChar, ColumnName = "Description", DataLength = -1, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.VarChar , ColumnName = "TableType", DataLength = 100, AllowNull = true },
            new SqlColumn { DataType = SqlDbType.VarChar , ColumnName = "StackTrace", DataLength = -1, AllowNull = true },
        ];

        loggerConfiguration.WriteTo.Logger(c =>
            c.Filter.ByIncludingOnly(evt => evt.Properties.GetValueOrDefault("TableType")?.ToString()?.Contains(AUDIT_TABLE_TYPE) ?? false)
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions()
                {
                    TableName = "LogAuditoria",
                    AutoCreateSqlTable = true
                },
                columnOptions: columnOptions)
            );

        return loggerConfiguration;
    }
}
```

## LoggerApplication
Implementação do LoggerApplication

```c#
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
...
```
## Chamada na Controller
Para chamar nas Controllers, Services, CommandHandlers entre outros, é necessário injetar a Interface ILoggerApplication<T>

```c#

[ApiController]
[Route("[controller]")]
public class LoggerSampleController(ILoggerApplication<LoggerSampleController> logger) : ControllerBase
{
    private readonly ILoggerApplication<LoggerSampleController> _logger = logger;

    [HttpPost("RegisterAllLogs")]
    public IActionResult RegisterAllLogs([FromBody]string diogo)
    {
        // Exemplo de registro de Information
        _logger.InfoApp("Teste de registro de informação para Application");
        _logger.InfoAudit("Teste de registro de informação para Audit", "Nome da Pessoa", "Alteração a pedido do Gestor");

        // Exemplo de registro de Warning
        _logger.WarnApp("Teste de registro de informação para Application");
        _logger.WarnAudit("Teste de registro de informação para Audit", "Nome da Pessoa", "Alteração a pedido do Gestor");

        // Exemplo de registro de Exception Level Error
        try
        {
            throw new Exception("Erro no sistema");
        }
        catch (Exception ex)
        {
            _logger.ErrorApp("Teste de registro de informação para Application", ex);
            _logger.ErrorAudit("Teste de registro de informação para Audit", "Nome da Pessoa", "Alteração a pedido do Gestor", ex);
        }

        return Ok();
    }
}
```
