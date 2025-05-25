using Rebus.Handlers;
using Thunders.TechTest.ApiService.Messages;

namespace Thunders.TechTest.ApiService.Handlers
{
    public class UtilizacaoProcessadaMessageHandler : IHandleMessages<UtilizacaoProcessadaMessage>
    {
        private readonly ILogger<UtilizacaoProcessadaMessageHandler> _logger;

        public UtilizacaoProcessadaMessageHandler(ILogger<UtilizacaoProcessadaMessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(UtilizacaoProcessadaMessage message)
        {
            _logger.LogInformation("Resultado processamento - Lote: {LoteId}, OK: {OK}, Erros: {Erros}",
                message.LoteId, message.TotalProcessadas, message.TotalComErro);

            await Task.CompletedTask;
        }
    }
}
