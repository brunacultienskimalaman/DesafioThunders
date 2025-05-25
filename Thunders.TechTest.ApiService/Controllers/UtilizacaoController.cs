using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;
using Thunders.TechTest.ApiService.Services;

namespace Thunders.TechTest.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UtilizacaoController : ControllerBase
{
    private readonly IUtilizacaoService _utilizacaoService;
    private readonly IValidator<UtilizacaoDto> _utilizacaoValidator;
    private readonly IValidator<UtilizacaoLoteDto> _loteValidator;
    private readonly ILogger<UtilizacaoController> _logger;

    public UtilizacaoController(
        IUtilizacaoService utilizacaoService,
        IValidator<UtilizacaoDto> utilizacaoValidator,
        IValidator<UtilizacaoLoteDto> loteValidator,
        ILogger<UtilizacaoController> logger)
    {
        _utilizacaoService = utilizacaoService;
        _utilizacaoValidator = utilizacaoValidator;
        _loteValidator = loteValidator;
        _logger = logger;
    }

   
    [HttpGet("recupera-utilizacoes")]
    public async Task<IActionResult> ObterUltimasUtilizacoes()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var ultimasUtilizacoes = await _utilizacaoService.ObterUltimasUtilizacoesDtoAsync();

            stopwatch.Stop();
            _logger.LogInformation("Últimas utilizações obtidas em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return Ok(new
            {
                Success = true,
                Message = "Últimas utilizações obtidas com sucesso",
                Data = ultimasUtilizacoes
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao obter últimas utilizações");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro interno do servidor",
                TraceId = Activity.Current?.Id
            });
        }
    }
        
    [HttpPost("Insere-utilizacao")]
    public async Task<IActionResult> ReceberUtilizacaoAsync([FromBody] UtilizacaoDto utilizacao)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageId = Guid.NewGuid();

        try
        {
            var validationResult = await _utilizacaoValidator.ValidateAsync(utilizacao);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Dados inválidos",
                    Errors = validationResult.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    })
                });
            }

            var resultado = await _utilizacaoService.ProcessarUtilizacaoViaRebus(utilizacao);

            stopwatch.Stop();
            _logger.LogInformation("Utilização enviada para processamento assíncrono. MessageId: {MessageId}, Tempo: {ElapsedMs}ms",
                messageId, stopwatch.ElapsedMilliseconds);

            return Accepted(new
            {
                Success = true,
                Message = "Utilização enviada para processamento assíncrono",
                Data = new
                {
                    MessageId = messageId,
                    Status = "Aceito",
                    DataEnvio = DateTime.UtcNow,
                    EstimativaProcessamento = "1-3 segundos",
                    TipoProcessamento = "Assíncrono",
                    TempoEnvio = $"{stopwatch.ElapsedMilliseconds}ms"
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao processar utilização assíncrona");
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro interno ao processar utilização assíncrona");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro interno do servidor",
                TraceId = Activity.Current?.Id
            });
        }
    }

    
    [HttpPost("Insere-lote")]
    public async Task<IActionResult> ReceberUtilizacoesLoteAsync([FromBody] UtilizacaoLoteDto lote)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageId = Guid.NewGuid();

        try
        {
            var validationResult = await _loteValidator.ValidateAsync(lote);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Dados do lote inválidos",
                    Errors = validationResult.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    })
                });
            }

            var resultado = await _utilizacaoService.ProcessarLoteViaRebus(lote);

            stopwatch.Stop();
            _logger.LogInformation("Lote de {Count} utilizações enviado para processamento assíncrono. MessageId: {MessageId}, Tempo: {ElapsedMs}ms",
                lote.Utilizacoes.Count, messageId, stopwatch.ElapsedMilliseconds);

            return Accepted(new
            {
                Success = true,
                Message = $"Lote de {lote.Utilizacoes.Count} utilizações enviado para processamento assíncrono",
                Data = new
                {
                    MessageId = messageId,
                    Status = "Aceito",
                    DataEnvio = DateTime.UtcNow,
                    TotalUtilizacoes = lote.Utilizacoes.Count,
                    EstimativaProcessamento = $"{Math.Ceiling(lote.Utilizacoes.Count / 100.0)} segundos",
                    TipoProcessamento = "Assíncrono Lote",
                    TempoEnvio = $"{stopwatch.ElapsedMilliseconds}ms"
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao processar lote assíncrono");
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro interno ao processar lote assíncrono");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro interno do servidor",
                TraceId = Activity.Current?.Id
            });
        }
    }
      
}