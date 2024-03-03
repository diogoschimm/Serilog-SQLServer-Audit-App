using Microsoft.AspNetCore.Mvc;
using SampleApiLoggerSerilog.Setup.LoggerSetup;

namespace SampleApiLoggerSerilog.Controllers;

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
