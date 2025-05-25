using System.ComponentModel.DataAnnotations;

namespace Thunders.TechTest.ApiService.Dtos.Praca
{
    public class PracaDto
    {

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo Nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Cidade é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo Cidade deve ter no máximo 100 caracteres.")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Estado é obrigatório.")]
        [StringLength(2, ErrorMessage = "O campo Estado deve ter exatamente 2 caracteres.")]
        public string Estado { get; set; } = string.Empty;

        public bool Ativa { get; set; } = true;
    }

}
