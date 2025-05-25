using System.ComponentModel.DataAnnotations;

namespace Thunders.TechTest.ApiService.Models
{
    public class Praca
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string Estado { get; set; } = string.Empty;

        public bool Ativa { get; set; } = true;

        // FK
        public virtual ICollection<UtilizacaoPedagio> Utilizacoes { get; set; } = new List<UtilizacaoPedagio>();
    }
}
