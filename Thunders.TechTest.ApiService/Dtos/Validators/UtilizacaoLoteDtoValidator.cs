using FluentValidation;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;

namespace Thunders.TechTest.ApiService.Dtos.Validators
{
    public class UtilizacaoLoteDtoValidator : AbstractValidator<UtilizacaoLoteDto>
    {
        public UtilizacaoLoteDtoValidator()
        {
            RuleFor(x => x.Utilizacoes)
                .NotEmpty()
                .WithMessage("Lista de utilizações é obrigatória")
                .Must(x => x.Count <= 10000)
                .WithMessage("Máximo de 10.000 utilizações por lote");

            RuleForEach(x => x.Utilizacoes)
                .SetValidator(new UtilizacaoDtoValidator());
        }
    }
}
