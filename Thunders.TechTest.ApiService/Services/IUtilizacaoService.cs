using Thunders.TechTest.ApiService.Dtos;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;
using Thunders.TechTest.ApiService.Messages;

namespace Thunders.TechTest.ApiService.Services
{
    public interface IUtilizacaoService
    {
        Task<UtilizacaoResultado> ProcessarUtilizacaoAsync(UtilizacaoDto utilizacao);
        Task<LoteResultado> ProcessarUtilizacoesLoteAsync(UtilizacaoLoteDto lote);
        Task<EstatisticasDto> ObterEstatisticasAsync();

        Task<List<UtilizacaoDto>> ObterUltimasUtilizacoesDtoAsync();
    }
}
