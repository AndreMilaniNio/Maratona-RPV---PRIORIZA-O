using Farol.Application.Prioritization;
using Farol.Application.Services;
using Farol.Domain.Entities;
using Farol.Domain.Enums;

namespace Farol.UnitTests.Priorizacao;

public class PoliticaOrdenacaoTests
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

    private static ChaveFila K(string numero, int nivel = 0, int rank = 3, TipoManutencao tipo = TipoManutencao.Corretiva, int pontos = 50,
        DateTimeOffset? prazo = null, DateTimeOffset? abertura = null, int municipio = 1) =>
        new(Guid.NewGuid(), municipio, nivel, rank, tipo, pontos, prazo, abertura ?? T0, numero);

    private static List<string> Ordenar(params ChaveFila[] chaves) => chaves.Order(PoliticaOrdenacao.Instancia).Select(c => c.Numero).ToList();

    [Fact]
    public void Precedencia_vem_antes_da_prioridade_e_da_pontuacao() =>
        Assert.Equal(["B", "A"], Ordenar(K("A", nivel: 0, rank: 1, pontos: 500), K("B", nivel: 2, rank: 3, pontos: 10)));

    [Fact]
    public void Prioridade_mais_urgente_primeiro() =>
        Assert.Equal(["B", "A"], Ordenar(K("A", rank: 3, pontos: 90), K("B", rank: 2, pontos: 60)));

    [Fact]
    public void Em_prioridade_equivalente_corretiva_antes_de_preventiva() =>
        Assert.Equal(["B", "A"], Ordenar(K("A", tipo: TipoManutencao.Preventiva, pontos: 99), K("B", tipo: TipoManutencao.Corretiva, pontos: 60)));

    [Fact]
    public void Preventiva_com_precedencia_supera_corretiva_comum() =>
        Assert.Equal(["A", "B"], Ordenar(K("A", nivel: 1, tipo: TipoManutencao.Preventiva), K("B", tipo: TipoManutencao.Corretiva)));

    [Fact]
    public void Desempates_sao_deterministicos_prazo_abertura_numero()
    {
        Assert.Equal(["B", "A"], Ordenar(K("A", prazo: T0.AddHours(2)), K("B", prazo: T0.AddHours(1))));
        Assert.Equal(["B", "A"], Ordenar(K("A", abertura: T0.AddMinutes(5)), K("B", abertura: T0)));
        Assert.Equal(["A-1", "A-2"], Ordenar(K("A-2"), K("A-1")));
        Assert.Equal(["A", "B"], Ordenar(K("B", prazo: null), K("A", prazo: T0)));
    }

    [Fact]
    public void Posicao_e_calculada_dentro_de_cada_cidade()
    {
        var a = K("A", municipio: 1, pontos: 10);
        var b = K("B", municipio: 1, pontos: 90);
        var c = K("C", municipio: 2, pontos: 5);
        var posicoes = PoliticaOrdenacao.Posicoes([a, b, c]);

        Assert.Equal(1, posicoes[b.Id]);
        Assert.Equal(2, posicoes[a.Id]);
        Assert.Equal(1, posicoes[c.Id]);
    }
}

public class CalculadoraPrazosTests
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    private static Prioridade P(CalendarioPrazo calendario) => new()
    {
        Codigo = "X", Nome = "X", Cor = "#000000", Calendario = calendario, PrazoTriagemMin = 30, PrazoDespachoMin = 120,
        PrazoInicioMin = 240, PrazoRestabelecimentoMin = null, PrazoConclusaoMin = 600,
    };

    [Fact]
    public void Calendario_corrido_soma_minutos()
    {
        var abertura = new DateTimeOffset(2026, 10, 10, 15, 0, 0, TimeSpan.Zero);
        var p = CalculadoraPrazos.Calcular(P(CalendarioPrazo.Corrido), abertura, Fuso, new HashSet<DateOnly>());

        Assert.Equal(abertura.AddMinutes(30), p.Triagem);
        Assert.Equal(abertura.AddMinutes(120), p.Despacho);
        Assert.Equal(abertura.AddMinutes(600), p.Conclusao);
        Assert.Null(p.Restabelecimento);
    }

    [Fact]
    public void Calendario_util_pula_fim_de_semana_e_conta_so_expediente()
    {
        // Sexta 17:00 (local) + 120 min úteis → segunda 09:00 (local).
        var sexta17Local = new DateTimeOffset(2026, 10, 9, 17, 0, 0, TimeSpan.FromHours(-3));
        var limite = CalculadoraPrazos.SomarMinutosUteis(sexta17Local, 120, Fuso, new HashSet<DateOnly>());
        var local = TimeZoneInfo.ConvertTime(limite, Fuso);

        Assert.Equal(new DateTime(2026, 10, 12, 9, 0, 0), local.DateTime);
    }

    [Fact]
    public void Calendario_util_pula_feriado()
    {
        // Domingo 11/10 + segunda 12/10 (feriado) → começa terça 13/10 08:00.
        var domingo = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.FromHours(-3));
        var limite = CalculadoraPrazos.SomarMinutosUteis(domingo, 60, Fuso, new HashSet<DateOnly> { new(2026, 10, 12) });

        Assert.Equal(new DateTime(2026, 10, 13, 9, 0, 0), TimeZoneInfo.ConvertTime(limite, Fuso).DateTime);
    }
}

public class FaixasEPendenciasTests
{
    private static readonly Dictionary<int, PrioridadeConfig> Prioridades = new()
    {
        [1] = new(1, "URG", "Urgente", 1), [2] = new(2, "MED", "Média", 2), [3] = new(3, "BAI", "Baixa", 3),
    };

    [Fact]
    public void Faixas_contiguas_de_zero_ao_infinito_sao_validas() =>
        Assert.Empty(PontuacaoService.ValidarFaixas([new(3, 0, 49), new(2, 50, 99), new(1, 100, null)], Prioridades));

    [Fact]
    public void Lacuna_e_rejeitada() =>
        Assert.Contains(PontuacaoService.ValidarFaixas([new(3, 0, 49), new(2, 60, 99), new(1, 100, null)], Prioridades), e => e.StartsWith("Lacuna"));

    [Fact]
    public void Sobreposicao_e_rejeitada() =>
        Assert.Contains(PontuacaoService.ValidarFaixas([new(3, 0, 60), new(2, 50, 99), new(1, 100, null)], Prioridades), e => e.StartsWith("Sobreposição"));

    [Fact]
    public void Inicio_diferente_de_zero_e_topo_fechado_sao_lacunas()
    {
        var erros = PontuacaoService.ValidarFaixas([new(3, 10, 49), new(1, 50, 200)], Prioridades);
        Assert.Equal(2, erros.Count(e => e.StartsWith("Lacuna")));
    }

    [Fact]
    public void Opcao_sem_pontos_em_criterio_habilitado_impede_publicacao()
    {
        var config = new ConfiguracaoClassificacao(1, 1, null, false,
            [
                new CriterioConfig("A", "Critério A", AgregacaoCriterio.Maximo, true, [new("X", "Opção X", null, false, false)]),
                new CriterioConfig("B", "Critério B", AgregacaoCriterio.Maximo, false, [new("Y", "Opção Y", null, false, false)]),
            ],
            [new FaixaConfig(3, 0, null)], [], Prioridades);

        var pendencias = PontuacaoService.Pendencias(config);
        Assert.Single(pendencias);
        Assert.Contains("Critério A › Opção X", pendencias[0]);
    }

    [Fact]
    public void Desconhecida_com_zero_pontos_sem_confirmacao_gera_alerta()
    {
        var config = new ConfiguracaoClassificacao(1, 1, null, false,
            [new CriterioConfig("A", "Critério A", AgregacaoCriterio.Maximo, true, [new("D", "Desconhecida", 0, false, true)])],
            [new FaixaConfig(3, 0, null)], [], Prioridades);
        Assert.Contains(PontuacaoService.Alertas(config), a => a.Contains("risco zero"));
    }
}
