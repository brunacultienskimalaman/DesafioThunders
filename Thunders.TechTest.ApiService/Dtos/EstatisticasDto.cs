
namespace Thunders.TechTest.ApiService.Dtos
{
    public class EstatisticasDto
    {
        public long TotalUtilizacoesHoje { get; set; }
        public long TotalUtilizacoesOntem { get; set; }
        public decimal MediaValorHoje { get; set; }
        public Dictionary<string, int> UtilizacoesPorTipoVeiculo { get; set; } = new();
        public Dictionary<string, int> UtilizacoesPorEstado { get; set; } = new();
        public DateTime UltimaAtualizacao { get; set; }
    }
}
