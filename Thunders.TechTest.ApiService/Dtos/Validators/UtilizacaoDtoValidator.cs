using FluentValidation;
using Thunders.TechTest.ApiService.Dtos.Utilizacao;

namespace Thunders.TechTest.ApiService.Dtos.Validators
{

    public class UtilizacaoDtoValidator : AbstractValidator<UtilizacaoDto>
    {
        public UtilizacaoDtoValidator()
        {
            RuleFor(x => x.DataUtilizacao)
                .NotEmpty()
                .WithMessage("Data e hora de utilização são obrigatórias")
                .Must(BeValidDate)
                .WithMessage("Data de utilização não pode ser no futuro")
                .Must(BeRecentDate)
                .WithMessage("Data de utilização não pode ser anterior a 1 ano");

            RuleFor(x => x.PracaId)
                .GreaterThan(0)
                .WithMessage("ID da praça deve ser maior que zero");

            RuleFor(x => x.Cidade)
                .NotEmpty()
                .WithMessage("Cidade é obrigatória")
                .MaximumLength(100)
                .WithMessage("Cidade deve ter no máximo 100 caracteres")
                .Matches(@"^[a-zA-ZÀ-ÿ\s\-\'\.]+$")
                .WithMessage("Cidade contém caracteres inválidos");

            RuleFor(x => x.Estado)
                .NotEmpty()
                .WithMessage("Estado é obrigatório")
                .Length(2)
                .WithMessage("Estado deve ter exatamente 2 caracteres")
                .Matches(@"^[A-Z]{2}$")
                .WithMessage("Estado deve conter apenas letras maiúsculas");

            RuleFor(x => x.ValorPago)
                .GreaterThan(0)
                .WithMessage("Valor pago deve ser maior que zero")
                .LessThanOrEqualTo(999999.99m)
                .WithMessage("Valor pago não pode exceder R$ 999.999,99");

            RuleFor(x => x.TipoVeiculo)
                .IsInEnum()
                .WithMessage("Tipo de veículo inválido");
        }

        private static bool BeValidDate(DateTime date)
        {
            return date <= DateTime.Now;
        }

        private static bool BeRecentDate(DateTime date)
        {
            return date >= DateTime.Now.AddYears(-1);
        }
    }
}
