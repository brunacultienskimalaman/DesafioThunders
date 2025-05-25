namespace Thunders.TechTest.ApiService.Dtos.Praca
{
    public class PracaResultado
    {
        public Guid Id { get; set; }
        public DateTime DataProcessamento { get; set; }
        public bool Sucesso { get; set; }
        public string? MensagemErro { get; set; }
    }
}
