using Farol.Application.DTOs;
using Farol.Domain.Rules;
using FluentValidation;

namespace Farol.Application.Validators;

/// <summary>Validações de estrutura e de combinação de respostas (seção 4.8, passos 1 e 2).</summary>
public class NovaSolicitacaoValidator : AbstractValidator<NovaSolicitacaoRequest>
{
    public NovaSolicitacaoValidator()
    {
        RuleFor(x => x.ChaveIdempotencia).NotEmpty();
        RuleFor(x => x.MunicipioId).GreaterThan(0).WithMessage("Selecione o município.");
        RuleFor(x => x.TipoOcorrenciaId).GreaterThan(0).WithMessage("Selecione o tipo de ocorrência.");
        RuleFor(x => x.TipoManutencao).IsInEnum();
        RuleFor(x => x.Canal).IsInEnum();
        RuleFor(x => x.Descricao).NotEmpty().WithMessage("Descreva a ocorrência.").MaximumLength(4000);
        RuleFor(x => x.Origem).MaximumLength(200);
        RuleFor(x => x.ProtocoloExterno).MaximumLength(100);

        RuleFor(x => x.Ucs).NotEmpty()
            .When(x => !x.UcNaoInformada)
            .WithMessage("Informe a UC ou marque \"Solicitante fora da própria residência / não sabe a UC\".");
        RuleFor(x => x.MotivoUcNaoInformada).NotEmpty().MaximumLength(300)
            .When(x => x.UcNaoInformada)
            .WithMessage("Informe por que a UC não foi informada.");
        RuleForEach(x => x.Ucs).NotEmpty().MaximumLength(30);

        RuleFor(x => x)
            .Must(TemMeioDeLocalizar)
            .When(x => x.UcNaoInformada)
            .WithName("Localizacao")
            .WithMessage("Informe ao menos um meio de localizar a ocorrência: rua, CEP ou coordenadas.");

        RuleFor(x => x.Localizacao.Cep)
            .Matches(@"^\d{5}-?\d{3}$").When(x => !string.IsNullOrWhiteSpace(x.Localizacao.Cep))
            .WithMessage("CEP deve ter 8 dígitos.");

        RuleFor(x => x.Localizacao.Latitude).InclusiveBetween(-90, 90)
            .WithMessage("Latitude deve estar entre -90 e 90.");
        RuleFor(x => x.Localizacao.Longitude).InclusiveBetween(-180, 180)
            .WithMessage("Longitude deve estar entre -180 e 180.");
        RuleFor(x => x.Localizacao)
            .Must(l => l.Latitude.HasValue == l.Longitude.HasValue)
            .WithMessage("Informe latitude e longitude juntas.");

        RuleFor(x => x.Rede.ConjuntoId).Null()
            .When(x => x.Rede.SubestacaoId is null)
            .WithMessage("Selecione o circuito (subestação) antes do conjunto elétrico.");

        RuleFor(x => x.Impacto.CondicoesSeguranca)
            .Must(c => c.Count <= 1 || !c.Any(Criterios.Seguranca.Exclusivas.Contains))
            .WithMessage("\"Sem risco adicional\" e \"Situação desconhecida\" não podem ser combinadas com outras condições.");
        RuleFor(x => x.Impacto.QuantidadePessoas).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Impacto.QuantidadeUcs).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Impacto.QuantidadeEquipamentos).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Impacto.DuracaoEstimadaMin).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Impacto)
            .Must(i => i.QuantidadePessoas is null || i.PessoasAfetadas == Criterios.Faixa.DeQuantidade(i.QuantidadePessoas.Value))
            .WithMessage("A quantidade de pessoas informada não corresponde à faixa selecionada.");
        RuleFor(x => x.Impacto)
            .Must(i => i.QuantidadeUcs is null || i.UcsAfetadas == Criterios.Faixa.DeQuantidade(i.QuantidadeUcs.Value))
            .WithMessage("A quantidade de unidades consumidoras não corresponde à faixa selecionada.");
        RuleFor(x => x.Impacto)
            .Must(i => i.CondicaoFornecimento != Criterios.Fornecimento.SemInterrupcao || i.DuracaoEstimadaMin is null or 0)
            .WithMessage("Duração de interrupção informada para ocorrência sem interrupção.");
    }

    private static bool TemMeioDeLocalizar(NovaSolicitacaoRequest x) =>
        !string.IsNullOrWhiteSpace(x.Localizacao.Logradouro) ||
        !string.IsNullOrWhiteSpace(x.Localizacao.Cep) ||
        (x.Localizacao.Latitude.HasValue && x.Localizacao.Longitude.HasValue);
}
