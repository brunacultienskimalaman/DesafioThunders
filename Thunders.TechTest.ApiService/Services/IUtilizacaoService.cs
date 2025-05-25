using Thunders.TechTest.ApiService.Dtos.Utilizacao;
using Thunders.TechTest.ApiService.Messages;

namespace Thunders.TechTest.ApiService.Services
{
    public interface IUtilizacaoService
    {
        Task<List<UtilizacaoDto>> ObterUltimasUtilizacoesDtoAsync();
        Task<UtilizacaoResultado> ProcessarUtilizacaoViaRebus(UtilizacaoDto utilizacao);
        Task<UtilizacaoProcessadaMessage> ProcessarLoteViaRebus(UtilizacaoLoteDto lote);
    }
}
