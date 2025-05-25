using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thunders.TechTest.ApiService.Models.Enums;

namespace Thunders.TechTest.ApiService.Models
{
    public class UtilizacaoPedagio
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public DateTime DataUtilizacao { get; set; }

        [Required]
        public int PracaId { get; set; }

        [Required]
        [StringLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorPago { get; set; }

        [Required]
        public TipoVeiculo TipoVeiculo { get; set; }

        public DateTime DataInsercao { get; set; } = DateTime.Now;

        // fk
        public virtual Praca? Praca { get; set; }
    }
}
