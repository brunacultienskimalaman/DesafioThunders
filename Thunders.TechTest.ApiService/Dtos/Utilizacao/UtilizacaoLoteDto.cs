using System.ComponentModel.DataAnnotations;

namespace Thunders.TechTest.ApiService.Dtos.Utilizacao
{
    public class UtilizacaoLoteDto
    {
        [Required(ErrorMessage = "Lista de utilizações é obrigatória")]
        [MinLength(1, ErrorMessage = "Deve conter pelo menos uma utilização")]
        [MaxLength(10000, ErrorMessage = "Máximo de 10.000 utilizações por lote")]
        public List<UtilizacaoDto> Utilizacoes { get; set; } = new();
    }
}
