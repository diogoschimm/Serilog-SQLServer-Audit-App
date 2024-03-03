using Serilog;
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
