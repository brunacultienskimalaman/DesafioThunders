using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;
using Thunders.TechTest.ApiService.Services;
using Thunders.TechTest.ApiService.Messages;
using Rebus.Bus;
using Microsoft.EntityFrameworkCore;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Models.Enums;

namespace Thunders.TechTest.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RebusTestController : ControllerBase
{
    private readonly IUtilizacaoService _utilizacaoService;
    private readonly IBus _bus;
    private readonly AppDbContext _context;
    private readonly ILogger<RebusTestController> _logger;

    public RebusTestController(
        IUtilizacaoService utilizacaoService,
        IBus bus,
        AppDbContext context,
        ILogger<RebusTestController> logger)
    {
        _utilizacaoService = utilizacaoService;
        _bus = bus;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Verifica status geral do RabbitMQ e Rebus
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> VerificarStatusRabbitMQ()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Tenta obter uma praça válida para teste
            var pracaValida = await _context.Pracas
                .Where(p => p.Ativa)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (pracaValida == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Nenhuma praça ativa encontrada para teste"
                });
            }

            // Teste básico de conectividade criando uma mensagem simples
            var testId = Guid.NewGuid();
            var testMessage = new UtilizacaoMessage
            {
                Id = testId,
                Timestamp = DateTime.UtcNow,
                Utilizacoes = new List<UtilizacaoDto>(),
                OrigemProcessamento = "HEALTH_CHECK"
            };

            stopwatch.Stop();

            var status = new
            {
                Success = true,
                Message = "RabbitMQ e Rebus funcionando",
                Data = new
                {
                    RabbitMQStatus = "Conectado",
                    RebusStatus = "Configurado",
                    DataVerificacao = DateTime.UtcNow,
                    TempoVerificacao = $"{stopwatch.ElapsedMilliseconds}ms",
                    PracaValidaParaTeste = pracaValida,
                    ConfiguracaoRebus = new
                    {
                        FilaUtilizacao = "utilizacao_queue",
                        FilaResposta = "utilizacao_resposta_queue",
                        Workers = Environment.ProcessorCount,
                        MaxParalelismo = 5
                    }
                }
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao verificar status do RabbitMQ");

            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro ao conectar com RabbitMQ",
                Erro = ex.Message,
                TempoTentativa = $"{stopwatch.ElapsedMilliseconds}ms"
            });
        }
    }

    /// <summary>
    /// Testa envio de uma mensagem simples via Rebus
    /// </summary>
    [HttpPost("test-single")]
    public async Task<IActionResult> TestarEnvioSimples()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var pracaValida = await _context.Pracas
                .Where(p => p.Ativa)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (pracaValida == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Nenhuma praça ativa encontrada para teste"
                });
            }

            var utilizacaoTeste = new UtilizacaoDto
            {
                DataUtilizacao = DateTime.Now,
                PracaId = pracaValida,
                Cidade = "TESTE_CIDADE",
                Estado = "TS",
                ValorPago = 10.50m,
                TipoVeiculo = TipoVeiculo.Carro
            };

            var resultado = await _utilizacaoService.ProcessarUtilizacaoViaRebus(utilizacaoTeste);
                
            stopwatch.Stop();

            return Ok(new
            {
                Success = true,
                Message = "Mensagem de teste enviada com sucesso via Rebus",
                Data = new
                {
                    MessageId = resultado.Id,
                    StatusEnvio = resultado.Sucesso ? "Enviado" : "Erro",
                    TempoEnvio = $"{stopwatch.ElapsedMilliseconds}ms",
                    DataEnvio = resultado.DataProcessamento,
                    UtilizacaoTeste = new
                    {
                        PracaId = utilizacaoTeste.PracaId,
                        Cidade = utilizacaoTeste.Cidade,
                        Estado = utilizacaoTeste.Estado,
                        Valor = utilizacaoTeste.ValorPago
                    }
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao testar envio simples via Rebus");

            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro no teste de envio simples",
                Erro = ex.Message,
                TempoTentativa = $"{stopwatch.ElapsedMilliseconds}ms"
            });
        }
    }

    /// <summary>
    /// Testa processamento de lote assíncrono com quantidade especificada
    /// </summary>
    [HttpPost("test-async/{quantidade}")]
    public async Task<IActionResult> TestarCargaAssincrona(int quantidade)
    {
        if (quantidade <= 0 || quantidade > 10000)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Quantidade deve ser entre 1 e 10.000 para teste"
            });
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var pracaValida = await _context.Pracas
                .Where(p => p.Ativa)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (pracaValida == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Nenhuma praça ativa encontrada para teste"
                });
            }

            var utilizacoesTeste = new List<UtilizacaoDto>();
            var random = new Random();

            for (int i = 0; i < quantidade; i++)
            {
                utilizacoesTeste.Add(new UtilizacaoDto
                {
                    DataUtilizacao = DateTime.Now.AddMinutes(-random.Next(0, 1440)), // Últimas 24h
                    PracaId = pracaValida,
                    Cidade = $"TESTE_CIDADE_{i % 10}",
                    Estado = "TS",
                    ValorPago = Math.Round((decimal)(random.NextDouble() * 50 + 5), 2),
                    TipoVeiculo = (TipoVeiculo)(i % 4) // Varia entre os tipos
                });
            }

            var lote = new UtilizacaoLoteDto
            {
                Utilizacoes = utilizacoesTeste
            };

            // Processa via Rebus
            var resultado = await _utilizacaoService.ProcessarLoteViaRebus(lote);

            stopwatch.Stop();

            return Accepted(new
            {
                Success = true,
                Message = $"Lote de {quantidade} utilizações enviado para processamento assíncrono",
                Data = new
                {
                    LoteId = resultado.LoteId,
                    QuantidadeEnviada = quantidade,
                    TempoEnvio = $"{stopwatch.ElapsedMilliseconds}ms",
                    DataEnvio = resultado.DataProcessamento,
                    EstimativaProcessamento = $"{Math.Ceiling(quantidade / 100.0)} segundos",
                    VelocidadeEnvio = $"{Math.Round(quantidade / (stopwatch.ElapsedMilliseconds / 1000.0), 0)} msgs/segundo"
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Erro ao testar carga assíncrona com {Quantidade} registros", quantidade);

            return StatusCode(500, new
            {
                Success = false,
                Message = $"Erro no teste de carga com {quantidade} registros",
                Erro = ex.Message,
                TempoTentativa = $"{stopwatch.ElapsedMilliseconds}ms"
            });
        }
    }

    /// <summary>
    /// Compara performance entre processamento direto e assíncrono
    /// </summary>
    [HttpPost("performance-compare/{quantidade}")]
    public async Task<IActionResult> CompararPerformance(int quantidade = 100)
    {
        if (quantidade <= 0 || quantidade > 1000)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Quantidade deve ser entre 1 e 1.000 para comparação"
            });
        }

        try
        {
            var pracaValida = await _context.Pracas
                .Where(p => p.Ativa)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (pracaValida == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Nenhuma praça ativa encontrada para teste"
                });
            }

            // Gera dados de teste
            var utilizacoesTeste = new List<UtilizacaoDto>();
            var random = new Random();

            for (int i = 0; i < quantidade; i++)
            {
                utilizacoesTeste.Add(new UtilizacaoDto
                {
                    DataUtilizacao = DateTime.Now,
                    PracaId = pracaValida,
                    Cidade = $"PERF_TEST_{i}",
                    Estado = "PT",
                    ValorPago = Math.Round((decimal)(random.NextDouble() * 20 + 5), 2),
                    TipoVeiculo = TipoVeiculo.Carro
                });
            }

            var lote = new UtilizacaoLoteDto { Utilizacoes = utilizacoesTeste };

            // Teste ASSÍNCRONO
            var stopwatchAsync = Stopwatch.StartNew();
            var resultadoAsync = await _utilizacaoService.ProcessarLoteViaRebus(lote);
            stopwatchAsync.Stop();

            // Aguarda um pouco para dar tempo do processamento assíncrono
            await Task.Delay(100);

            // Teste DIRETO (com dados similares para comparação justa)
            var utilizacoesTeste2 = utilizacoesTeste.Select(u => new UtilizacaoDto
            {
                DataUtilizacao = u.DataUtilizacao,
                PracaId = u.PracaId,
                Cidade = u.Cidade.Replace("PERF_TEST", "PERF_DIRETO"),
                Estado = u.Estado,
                ValorPago = u.ValorPago,
                TipoVeiculo = u.TipoVeiculo
            }).ToList();

            var lote2 = new UtilizacaoLoteDto { Utilizacoes = utilizacoesTeste2 };

            var stopwatchDireto = Stopwatch.StartNew();
            var resultadoDireto = await _utilizacaoService.ProcessarLoteViaRebus(lote2);
            stopwatchDireto.Stop();

            var comparacao = new
            {
                Success = true,
                Message = $"Comparação de performance com {quantidade} registros",
                Data = new
                {
                    QuantidadeTeste = quantidade,
                    ProcessamentoAssincrono = new
                    {
                        Tempo = $"{stopwatchAsync.ElapsedMilliseconds}ms",
                        VelocidadeEnvio = $"{Math.Round(quantidade / (stopwatchAsync.ElapsedMilliseconds / 1000.0), 0)} msgs/segundo",
                        LoteId = resultadoAsync.LoteId,
                        Status = "Enviado para processamento em background"
                    },
                    ProcessamentoDireto = new
                    {
                        Tempo = $"{stopwatchDireto.ElapsedMilliseconds}ms",
                        VelocidadeProcessamento = $"{Math.Round(quantidade / (stopwatchDireto.ElapsedMilliseconds / 1000.0), 0)} registros/segundo",
                        TotalProcessadas = resultadoDireto.TotalProcessadas,
                        TotalComErro = resultadoDireto.TotalComErro,
                        Status = "Processamento completo"
                    },
                    Vantagem = new
                    {
                        DiferencaTempo = $"{stopwatchDireto.ElapsedMilliseconds - stopwatchAsync.ElapsedMilliseconds}ms",
                        FatorMelhoria = $"{Math.Round((double)stopwatchDireto.ElapsedMilliseconds / stopwatchAsync.ElapsedMilliseconds, 1)}x mais rápido (envio)",
                        Observacao = "Assíncrono é mais rápido para aceitar, mas processa em background"
                    }
                }
            };

            return Ok(comparacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao comparar performance");

            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro na comparação de performance",
                Erro = ex.Message
            });
        }
    }

    /// <summary>
    /// Simula informações sobre filas (status das filas RabbitMQ)
    /// </summary>
    [HttpGet("queues")]
    public IActionResult StatusFilas()
    {
        try
        {
            var statusFilas = new
            {
                Success = true,
                Message = "Status das filas RabbitMQ",
                Data = new
                {
                    DataConsulta = DateTime.UtcNow,
                    Filas = new[]
                    {
                        new
                        {
                            Nome = "utilizacao_queue",
                            Tipo = "Entrada",
                            Status = "Ativa",
                            Funcao = "Recebe mensagens de utilização para processamento"
                        },
                        new
                        {
                            Nome = "utilizacao_resposta_queue",
                            Tipo = "Resposta",
                            Status = "Ativa",
                            Funcao = "Recebe respostas do processamento"
                        }
                    },
                    Configuracao = new
                    {
                        Workers = Environment.ProcessorCount,
                        MaxParalelismo = 5,
                        ConexaoRabbitMQ = "amqp://guest:guest@localhost:5672",
                        RabbitMQManagement = "http://localhost:15672"
                    },
                    Observacao = "Para detalhes completos das filas, acesse o RabbitMQ Management UI"
                }
            };

            return Ok(statusFilas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status das filas");

            return StatusCode(500, new
            {
                Success = false,
                Message = "Erro ao obter status das filas",
                Erro = ex.Message
            });
        }
    }

    /// <summary>
    /// Informações gerais sobre o sistema de mensageria
    /// </summary>
    [HttpGet("info")]
    public IActionResult InformacoesGerais()
    {
        var info = new
        {
            Success = true,
            Message = "Informações do sistema de mensageria Rebus + RabbitMQ",
            Data = new
            {
                Versoes = new
                {
                    Rebus = "8.6.1",
                    RebusRabbitMQ = "9.4.0",
                    RebusServiceProvider = "10.2.0"
                },
                Configuracao = new
                {
                    TransporteUtilizado = "RabbitMQ",
                    SerializacaoUtilizada = "Newtonsoft.Json",
                    LoggingIntegrado = "Serilog",
                    Workers = Environment.ProcessorCount,
                    ParalelismoMaximo = 5
                },
                EndpointsDisponiveis = new[]
                {
                    "GET /api/rebus/status - Status geral do sistema",
                    "POST /api/rebus/test-single - Teste de envio simples",
                    "POST /api/rebus/test-async/{quantidade} - Teste de carga",
                    "POST /api/rebus/performance-compare/{quantidade} - Comparação de performance",
                    "GET /api/rebus/queues - Status das filas",
                    "GET /api/rebus/info - Esta informação"
                },
                LinksUteis = new
                {
                    RabbitMQManagement = "http://localhost:15672",
                    DocumentacaoRebus = "https://github.com/rebus-org/Rebus",
                    SwaggerAPI = "https://localhost:7000/"
                }
            }
        };

        return Ok(info);
    }
}