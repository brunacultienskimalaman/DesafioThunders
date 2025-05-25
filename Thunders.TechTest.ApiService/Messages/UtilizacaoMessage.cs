using Thunders.TechTest.ApiService.Dtos.Utilizacao;

namespace Thunders.TechTest.ApiService.Messages
{
    public class UtilizacaoMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public List<UtilizacaoDto> Utilizacoes { get; set; } = new();
        public string? OrigemProcessamento { get; set; }
    }
}
