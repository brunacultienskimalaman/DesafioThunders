using System.ComponentModel.DataAnnotations;
using Thunders.TechTest.ApiService.Models.Enums;

namespace Thunders.TechTest.ApiService.Dtos.Utilizacao
{
    public class UtilizacaoDto
    {
        [Required(ErrorMessage = "Data e hora de utilização são obrigatórias")]
        public DateTime DataUtilizacao { get; set; }

        [Required(ErrorMessage = "ID da praça é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "ID da praça deve ser maior que zero")]
        public int PracaId { get; set; }

        [Required(ErrorMessage = "Cidade é obrigatória")]
        [StringLength(100, ErrorMessage = "Cidade deve ter no máximo 100 caracteres")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Estado é obrigatório")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter exatamente 2 caracteres")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valor pago é obrigatório")]
        [Range(0.01, 999999.99, ErrorMessage = "Valor pago deve estar entre R$ 0,01 e R$ 999.999,99")]
        public decimal ValorPago { get; set; }

        [Required(ErrorMessage = "Tipo de veículo é obrigatório")]
        public TipoVeiculo TipoVeiculo { get; set; }
    }
}
