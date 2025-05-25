namespace Thunders.TechTest.ApiService.Messages
{
    public class UtilizacaoProcessadaMessage
    {
        public Guid LoteId { get; set; }
        public int TotalProcessadas { get; set; }
        public int TotalComErro { get; set; }
        public DateTime DataProcessamento { get; set; }
        public List<string> Erros { get; set; } = new();
    }
}
