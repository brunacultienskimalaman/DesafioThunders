using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Thunders.TechTest.ApiService.Dtos.Praca;
using Thunders.TechTest.ApiService.Services;

namespace Thunders.TechTest.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PracaController: ControllerBase
    {
        private readonly IPracaService _PracaService;
        private readonly IValidator<PracaDto> _PracaValidator;
        private readonly ILogger<PracaController> _logger;

        public PracaController(IPracaService pracaService, IValidator<PracaDto> pracaValidator, 
            ILogger<PracaController> logger)
        {
            _PracaService = pracaService;
            _PracaValidator = pracaValidator;
            _logger = logger;
        }


        [HttpGet("ultimas")]
        public async Task<IActionResult> ObterUltimasPracas()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var ultimasPracas = await _PracaService.ObterUltimasPracasPorIdAsync();

                stopwatch.Stop();
                _logger.LogInformation("Últimas praças obtidas em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return Ok(new
                {
                    Success = true,
                    Message = "Últimas praças obtidas com sucesso",
                    Data = ultimasPracas
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erro ao obter últimas praças");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro interno do servidor",
                    TraceId = Activity.Current?.Id
                });
            }
        }

        [HttpPost("single")]
        public async Task<IActionResult> ReceberPraca([FromBody] PracaDto Praca)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var validationResult = await _PracaValidator.ValidateAsync(Praca);
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

                var resultado = await _PracaService.ProcessarPracaAsync(Praca);

                stopwatch.Stop();
                _logger.LogInformation("Praca única processada em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return Ok(new
                {
                    Success = true,
                    Message = "Praca recebida com sucesso",
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
                _logger.LogWarning(ex, "Erro de validação ao processar Praca única");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erro interno ao processar Praca única");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro interno do servidor",
                    TraceId = Activity.Current?.Id
                });
            }
        }
    }
}
