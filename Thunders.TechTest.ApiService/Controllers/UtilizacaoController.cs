// Controllers/UtilizacaoController.cs
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

    [HttpGet("ultimas")]
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

    /// <summary>
    /// Recebe uma única utilização de pedágio
    /// </summary>
    [HttpPost("single")]
    public async Task<IActionResult> ReceberUtilizacao([FromBody] UtilizacaoDto utilizacao)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validação
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

            // Processa a utilização
            var resultado = await _utilizacaoService.ProcessarUtilizacaoAsync(utilizacao);

            stopwatch.Stop();
            _logger.LogInformation("Utilização única processada em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return Ok(new
            {
                Success = true,
                Message = "Utilização recebida com sucesso",
                Data = new
                {
                    Id = resultado.Id,
                    DataProcessamento = resultado.DataProcessamento,
                    TempoProcessamento = $"{stopwatch.ElapsedMilliseconds}ms"
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao processar utilização única");
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro interno ao processar utilização única");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro interno do servidor",
                TraceId = Activity.Current?.Id
            });
        }
    }

    /// <summary>
    /// Recebe um lote de utilizações de pedágio
    /// </summary>
    [HttpPost("batch")]
    public async Task<IActionResult> ReceberUtilizacoesBatch([FromBody] UtilizacaoLoteDto lote)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validação do lote
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

            // Processa o lote
            var resultado = await _utilizacaoService.ProcessarUtilizacoesLoteAsync(lote);

            stopwatch.Stop();
            _logger.LogInformation("Lote de {Count} utilizações processado em {ElapsedMs}ms",
                lote.Utilizacoes.Count, stopwatch.ElapsedMilliseconds);

            return Ok(new
            {
                Success = true,
                Message = $"Lote de {lote.Utilizacoes.Count} utilizações recebido com sucesso",
                Data = new
                {
                    LoteId = resultado.LoteId,
                    TotalUtilizacoes = resultado.TotalProcessadas,
                    UtilizacoesComErro = resultado.TotalComErro,
                    DataProcessamento = resultado.DataProcessamento,
                    TempoProcessamento = $"{stopwatch.ElapsedMilliseconds}ms"
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao processar lote de utilizações");
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro interno ao processar lote de utilizações");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro interno do servidor",
                TraceId = Activity.Current?.Id
            });
        }
    }

    /// <summary>
    /// Consulta estatísticas de processamento
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> ObterEstatisticas()
    {
        try
        {
            var stats = await _utilizacaoService.ObterEstatisticasAsync();

            return Ok(new
            {
                Success = true,
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas");
            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }
}