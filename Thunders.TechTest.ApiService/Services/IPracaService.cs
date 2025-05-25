using Thunders.TechTest.ApiService.Dtos.Praca;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;

namespace Thunders.TechTest.ApiService.Services
{
    public interface IPracaService
    {
        Task<PracaResultado> ProcessarPracaAsync(PracaDto praca);
        Task<List<PracaDto>> ObterUltimasPracasPorIdAsync();
    }
}
