using Farol.Application.DTOs;
using Farol.Application.Validators;
using Farol.Domain.Enums;
using Farol.Domain.Rules;
using Farol.Domain.ValueObjects;

namespace Farol.UnitTests.Dominio;

public class NumeroTransformadorTests
{
    [Fact]
    public void Separa_localidade_e_numero_local_preservando_zeros()
    {
        Assert.True(NumeroTransformador.TentarCriar("0010042", 12, out var n, out _));
        Assert.Equal("001", n!.CodigoLocalidade);
        Assert.Equal("0042", n.NumeroLocal);
        Assert.Equal("0010042", n.Completo);
    }

    [Theory]
    [InlineData("123", "ao menos 4")]
    [InlineData("12A45", "somente dígitos")]
    [InlineData("1234567890123", "no máximo 12")]
    [InlineData("", "Informe")]
    public void Recusa_numeros_invalidos(string numero, string trecho)
    {
        Assert.False(NumeroTransformador.TentarCriar(numero, 12, out _, out var erro));
        Assert.Contains(trecho, erro);
    }
}

public class CoordenadasTests
{
    [Theory]
    [InlineData("-21.530443, -42.638946", -21.530443, -42.638946)]
    [InlineData("https://maps.google.com/?q=-21.53,-42.63", -21.53, -42.63)]
    [InlineData("https://www.google.com/maps/@-21.5304,-42.6389,17z", -21.5304, -42.6389)]
    [InlineData("21°31'49.594\"S 42°38'20.207\"W", -21.530443, -42.638946)]
    public void Interpreta_formatos_colados(string texto, double lat, double lng)
    {
        Assert.True(InterpretadorCoordenadas.TentarInterpretar(texto, out var c));
        Assert.Equal(lat, c.Latitude, 5);
        Assert.Equal(lng, c.Longitude, 5);
    }

    [Fact]
    public void Texto_sem_coordenadas_nao_e_interpretado() =>
        Assert.False(InterpretadorCoordenadas.TentarInterpretar("rua das flores", out _));

    [Fact]
    public void Detecta_intervalos_e_territorio()
    {
        Assert.False(new Coordenadas(95, 10).DentroDosLimites);
        Assert.False(new Coordenadas(48.85, 2.35).DentroDoBrasil);
        Assert.True(new Coordenadas(-21.53, -42.64).DentroDoBrasil);
    }

    [Fact]
    public void Distancia_haversine_aproximada()
    {
        var d = new Coordenadas(-21.53, -42.64).DistanciaKm(new Coordenadas(-21.54, -42.64));
        Assert.InRange(d, 1.10, 1.12);
    }
}

public class MaquinaEstadosTests
{
    [Theory]
    [InlineData(StatusOrdemServico.AguardandoDespacho, StatusOrdemServico.Cancelada, true)]
    [InlineData(StatusOrdemServico.EmExecucao, StatusOrdemServico.Cancelada, false)]
    [InlineData(StatusOrdemServico.Concluida, StatusOrdemServico.EmExecucao, false)]
    [InlineData(StatusOrdemServico.Concluida, StatusOrdemServico.AguardandoDespacho, true)]
    [InlineData(StatusOrdemServico.Cancelada, StatusOrdemServico.AguardandoDespacho, false)]
    public void Transicoes_manuais(StatusOrdemServico de, StatusOrdemServico para, bool permitido) =>
        Assert.Equal(permitido, MaquinaEstadosOrdemServico.PodeTransitarManualmente(de, para));

    [Fact]
    public void Cancelamento_e_reabertura_exigem_justificativa()
    {
        Assert.True(MaquinaEstadosOrdemServico.ExigeJustificativa(StatusOrdemServico.Aberta, StatusOrdemServico.Cancelada));
        Assert.True(MaquinaEstadosOrdemServico.ExigeJustificativa(StatusOrdemServico.Concluida, StatusOrdemServico.AguardandoDespacho));
        Assert.True(MaquinaEstadosOrdemServico.EhReabertura(StatusOrdemServico.Concluida, StatusOrdemServico.AguardandoDespacho));
    }

    [Fact]
    public void Prazo_corrente_segue_o_status()
    {
        Assert.Equal(TipoPrazo.Triagem, MaquinaEstadosOrdemServico.PrazoCorrente(StatusOrdemServico.EmTriagem));
        Assert.Equal(TipoPrazo.Despacho, MaquinaEstadosOrdemServico.PrazoCorrente(StatusOrdemServico.AguardandoDespacho));
        Assert.Equal(TipoPrazo.Inicio, MaquinaEstadosOrdemServico.PrazoCorrente(StatusOrdemServico.EquipeACaminho));
        Assert.Equal(TipoPrazo.Conclusao, MaquinaEstadosOrdemServico.PrazoCorrente(StatusOrdemServico.EmExecucao));
        Assert.Null(MaquinaEstadosOrdemServico.PrazoCorrente(StatusOrdemServico.Concluida));
    }
}

public class NovaSolicitacaoValidatorTests
{
    private static NovaSolicitacaoRequest Base() => new()
    {
        ChaveIdempotencia = Guid.NewGuid(), MunicipioId = 1, TipoOcorrenciaId = 1, Descricao = "Sem energia", Ucs = ["123456"],
    };

    [Fact]
    public void Uc_e_obrigatoria_quando_nao_marcado_nao_sabe_a_uc() =>
        Assert.False(new NovaSolicitacaoValidator().Validate(Base() with { Ucs = [] }).IsValid);

    [Fact]
    public void Nao_sabe_a_uc_exige_motivo_e_algum_meio_de_localizar()
    {
        var v = new NovaSolicitacaoValidator();
        var semNada = Base() with { Ucs = [], UcNaoInformada = true, MotivoUcNaoInformada = "Na rua" };
        Assert.Contains(v.Validate(semNada).Errors, e => e.ErrorMessage.Contains("meio de localizar"));

        var comCep = semNada with { Localizacao = new LocalizacaoInput { Cep = "00110398" } };
        Assert.True(v.Validate(comCep).IsValid);

        var semMotivo = comCep with { MotivoUcNaoInformada = null };
        Assert.False(v.Validate(semMotivo).IsValid);
    }

    [Fact]
    public void Condicoes_exclusivas_nao_combinam()
    {
        var r = Base() with { Impacto = new ImpactoInput { CondicoesSeguranca = [Criterios.Seguranca.SemRiscoAdicional, Criterios.Seguranca.RiscoChoque] } };
        Assert.False(new NovaSolicitacaoValidator().Validate(r).IsValid);
    }

    [Fact]
    public void Coordenadas_fora_do_intervalo_e_cep_invalido_sao_recusados()
    {
        var v = new NovaSolicitacaoValidator();
        Assert.False(v.Validate(Base() with { Localizacao = new LocalizacaoInput { Latitude = -95, Longitude = 10 } }).IsValid);
        Assert.False(v.Validate(Base() with { Localizacao = new LocalizacaoInput { Latitude = -21 } }).IsValid);
        Assert.False(v.Validate(Base() with { Localizacao = new LocalizacaoInput { Cep = "1234" } }).IsValid);
    }

    [Fact]
    public void Quantidade_precisa_bater_com_a_faixa()
    {
        var r = Base() with { Impacto = new ImpactoInput { PessoasAfetadas = Criterios.Faixa.Ate10, QuantidadePessoas = 50 } };
        Assert.False(new NovaSolicitacaoValidator().Validate(r).IsValid);
    }

    [Fact]
    public void Conjunto_exige_circuito() =>
        Assert.False(new NovaSolicitacaoValidator().Validate(Base() with { Rede = new RedeInput { ConjuntoId = 3 } }).IsValid);
}
