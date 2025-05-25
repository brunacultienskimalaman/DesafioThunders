using Rebus.Bus;
using Rebus.Handlers;
using System.Diagnostics;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Messages;
using Thunders.TechTest.ApiService.Models;

namespace Thunders.TechTest.ApiService.Handlers
{
    public class UtilizacaoMessageHandler : IHandleMessages<UtilizacaoMessage>
    {
        private readonly AppDbContext _context;
        private readonly IBus _bus;
        private readonly ILogger<UtilizacaoMessageHandler> _logger;

        public UtilizacaoMessageHandler(
            AppDbContext context,
            IBus bus,
            ILogger<UtilizacaoMessageHandler> logger)
        {
            _context = context;
            _bus = bus;
            _logger = logger;
        }

        public async Task Handle(UtilizacaoMessage message)
        {
            var stopwatch = Stopwatch.StartNew();
            var erros = new List<string>();
            var processadas = 0;

            _logger.LogInformation("Processando lote {LoteId} com {Total} utilizações",
                message.Id, message.Utilizacoes.Count);

            try
            {
                foreach (var utilizacaoDto in message.Utilizacoes)
                {
                    try
                    {
                        var utilizacao = new UtilizacaoPedagio
                        {
                            DataUtilizacao = utilizacaoDto.DataUtilizacao,
                            PracaId = utilizacaoDto.PracaId,
                            Cidade = utilizacaoDto.Cidade,
                            Estado = utilizacaoDto.Estado,
                            ValorPago = utilizacaoDto.ValorPago,
                            TipoVeiculo = utilizacaoDto.TipoVeiculo,
                            DataInsercao = DateTime.UtcNow
                        };

                        _context.Utilizacoes.Add(utilizacao);
                        processadas++;
                    }
                    catch (Exception ex)
                    {
                        erros.Add($"Erro ao processar item: {ex.Message}");
                        _logger.LogError(ex, "Erro ao processar utilização");
                    }
                }


                if (processadas > 0)
                    await _context.SaveChangesAsync();


                stopwatch.Stop();

                var resposta = new UtilizacaoProcessadaMessage
                {
                    LoteId = message.Id,
                    TotalProcessadas = processadas,
                    TotalComErro = erros.Count,
                    DataProcessamento = DateTime.UtcNow,
                    Erros = erros
                };

                await _bus.Reply(resposta);

                _logger.LogInformation("Lote {LoteId} processado: {Processadas} OK, {Erros} erros em {Tempo}ms",
                    message.Id, processadas, erros.Count, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao processar lote {LoteId}", message.Id);

                var respostaErro = new UtilizacaoProcessadaMessage
                {
                    LoteId = message.Id,
                    TotalProcessadas = 0,
                    TotalComErro = message.Utilizacoes.Count,
                    DataProcessamento = DateTime.UtcNow,
                    Erros = new List<string> { $"Erro crítico: {ex.Message}" }
                };

                await _bus.Reply(respostaErro);
                throw; //retry
            }
        }
    }
}
