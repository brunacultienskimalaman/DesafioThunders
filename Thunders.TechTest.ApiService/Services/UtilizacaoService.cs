using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;
using Thunders.TechTest.ApiService.Messages;

namespace Thunders.TechTest.ApiService.Services
{
    public class UtilizacaoService : IUtilizacaoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UtilizacaoService> _logger;
        private readonly IBus _bus;

        public UtilizacaoService(
            AppDbContext context,
            ILogger<UtilizacaoService> logger,
            IBus bus)
        {
            _context = context;
            _logger = logger;
            _bus = bus;
        }

        public async Task<UtilizacaoResultado> ProcessarUtilizacaoViaRebus(UtilizacaoDto utilizacao)
        {
            try
            {
                var messageId = Guid.NewGuid();

                var message = new UtilizacaoMessage
                {
                    Id = messageId,
                    Timestamp = DateTime.UtcNow,
                    Utilizacoes = new List<UtilizacaoDto> { utilizacao },
                    OrigemProcessamento = "API_Single_Async"
                };

                await _bus.Send(message);

                _logger.LogInformation("Utilização enviada para fila RabbitMQ. MessageId: {MessageId}, PracaId: {PracaId}",
                    messageId, utilizacao.PracaId);

                return new UtilizacaoResultado
                {
                    Id = messageId,
                    DataProcessamento = DateTime.UtcNow,
                    Sucesso = true,
                    MensagemErro = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar utilização para fila RabbitMQ");
                return new UtilizacaoResultado
                {
                    Id = Guid.NewGuid(),
                    DataProcessamento = DateTime.UtcNow,
                    Sucesso = false,
                    MensagemErro = $"Erro ao enviar para fila: {ex.Message}"
                };
            }
        }

        public async Task<UtilizacaoProcessadaMessage> ProcessarLoteViaRebus(UtilizacaoLoteDto lote)
        {
            try
            {
                var messageId = Guid.NewGuid();

                var message = new UtilizacaoMessage
                {
                    Id = messageId,
                    Timestamp = DateTime.UtcNow,
                    Utilizacoes = lote.Utilizacoes,
                    OrigemProcessamento = "API_Lote_Async"
                };

                await _bus.Send(message);

                _logger.LogInformation("Lote enviado para fila RabbitMQ. MessageId: {MessageId}, Total: {Total}",
                    messageId, lote.Utilizacoes.Count);

                return new UtilizacaoProcessadaMessage
                {
                    LoteId = messageId,
                    TotalProcessadas = 0,
                    TotalComErro = 0,
                    DataProcessamento = DateTime.UtcNow,
                    Erros = new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar lote para fila RabbitMQ");
                return new UtilizacaoProcessadaMessage
                {
                    LoteId = Guid.NewGuid(),
                    TotalProcessadas = 0,
                    TotalComErro = lote.Utilizacoes.Count,
                    DataProcessamento = DateTime.UtcNow,
                    Erros = new List<string> { $"Erro ao enviar para fila: {ex.Message}" }
                };
            }
        }

        public async Task<List<UtilizacaoDto>> ObterUltimasUtilizacoesDtoAsync()
        {
            try
            {
                var ultimasUtilizacoes = await _context.Utilizacoes
                    .OrderByDescending(u => u.DataInsercao)
                    .Take(10)
                    .Select(u => new UtilizacaoDto
                    {
                        DataUtilizacao = u.DataUtilizacao,
                        PracaId = u.PracaId,
                        Cidade = u.Cidade,
                        Estado = u.Estado,
                        ValorPago = u.ValorPago,
                        TipoVeiculo = u.TipoVeiculo
                    })
                    .ToListAsync();

                return ultimasUtilizacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter últimas utilizações");
                return new List<UtilizacaoDto>();
            }
        }

    }
}