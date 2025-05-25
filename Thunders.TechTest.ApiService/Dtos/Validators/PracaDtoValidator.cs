using FluentValidation;
using Thunders.TechTest.ApiService.Dtos.Praca;

namespace Thunders.TechTest.ApiService.Dtos.Validators
{
    public class PracaDtoValidator : AbstractValidator<PracaDto>
    {
        public PracaDtoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty()
                .WithMessage("O campo Nome é obrigatório.")
                .MaximumLength(100)
                .WithMessage("O campo Nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Cidade)
                .NotEmpty()
                .WithMessage("O campo Cidade é obrigatório.")
                .MaximumLength(100)
                .WithMessage("O campo Cidade deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Estado)
                .NotEmpty()
                .WithMessage("O campo Estado é obrigatório.")
                .Length(2)
                .WithMessage("O campo Estado deve ter exatamente 2 caracteres.");
        }
    }
}
