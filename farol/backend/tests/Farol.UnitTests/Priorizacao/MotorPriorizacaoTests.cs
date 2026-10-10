using Farol.Application.Prioritization;
using Farol.Application.Services;
using Farol.Domain.Entities;
using Farol.Domain.Enums;
using C = Farol.Domain.Rules.Criterios;

namespace Farol.UnitTests.Priorizacao;

public class MotorPriorizacaoTests
{
    private static readonly Dictionary<int, PrioridadeConfig> Prioridades = new()
    {
        [1] = new(1, "URG", "Urgente", 1),
        [2] = new(2, "ALT", "Alta", 2),
        [3] = new(3, "BAI", "Baixa", 3),
    };

    private static ConfiguracaoClassificacao Config(
        IEnumerable<CriterioConfig>? criterios = null, IEnumerable<RegraConfig>? regras = null, IEnumerable<FaixaConfig>? faixas = null) =>
        new(10, 3, null, false,
            (criterios ?? Criterios()).ToList(),
            (faixas ?? [new FaixaConfig(3, 0, 49), new FaixaConfig(2, 50, 99), new FaixaConfig(1, 100, null)]).ToList(),
            (regras ?? []).ToList(),
            Prioridades);

    private static List<CriterioConfig> Criterios() =>
    [
        new(C.RiscoSeguranca, "Risco", AgregacaoCriterio.Soma, true,
        [
            new(C.Seguranca.RiscoChoque, "Choque", 40, false, false),
            new(C.Seguranca.RiscoCirculacao, "Circulação", 20, false, false),
            new(C.Seguranca.SemRiscoAdicional, "Sem risco", 0, false, false),
            new(C.Seguranca.Desconhecida, "Desconhecida", 15, true, true),
        ]),
        new(C.PessoasAfetadas, "Pessoas", AgregacaoCriterio.Maximo, true,
        [
            new(C.Faixa.Ate10, "Até 10", 5, false, false),
            new(C.Faixa.MilOuMais, "1000+", 45, false, false),
            new(C.Faixa.Desconhecida, "Desconhecida", 15, true, true),
        ]),
        new(C.TipoManutencao, "Manutenção", AgregacaoCriterio.Maximo, true,
        [
            new(C.Manutencao.Corretiva, "Corretiva", 10, false, false),
            new(C.Manutencao.Preventiva, "Preventiva", 0, false, false),
        ]),
    ];

    private static FatosOcorrencia Fatos(params (string Criterio, string[] Opcoes)[] respostas) =>
        new(respostas.ToDictionary(r => r.Criterio, r => (IReadOnlyList<string>)r.Opcoes));

    [Fact]
    public void Mesma_entrada_e_mesma_versao_produzem_o_mesmo_resultado()
    {
        var fatos = Fatos((C.RiscoSeguranca, [C.Seguranca.RiscoCirculacao]), (C.PessoasAfetadas, [C.Faixa.MilOuMais]), (C.TipoManutencao, [C.Manutencao.Corretiva]));
        var a = MotorPriorizacao.Classificar(fatos, Config());
        var b = MotorPriorizacao.Classificar(fatos, Config());

        Assert.Equal(a.Pontuacao, b.Pontuacao);
        Assert.Equal(a.Prioridade, b.Prioridade);
        Assert.Equal(a.MotivoPrincipal, b.MotivoPrincipal);
        Assert.Equal(a.Itens.Select(i => (i.CriterioCodigo, i.Pontos)), b.Itens.Select(i => (i.CriterioCodigo, i.Pontos)));
    }

    [Fact]
    public void Pontuacao_detalhada_soma_exatamente_a_total()
    {
        var r = MotorPriorizacao.Classificar(
            Fatos((C.RiscoSeguranca, [C.Seguranca.RiscoChoque, C.Seguranca.RiscoCirculacao]), (C.PessoasAfetadas, [C.Faixa.Ate10]), (C.TipoManutencao, [C.Manutencao.Corretiva])),
            Config());

        Assert.Equal(r.Itens.Sum(i => i.Pontos), r.Pontuacao);
        Assert.Equal(60 + 5 + 10, r.Pontuacao);
    }

    [Fact]
    public void Criterio_com_agregacao_soma_soma_as_opcoes_e_maximo_pega_a_maior()
    {
        var soma = MotorPriorizacao.Classificar(Fatos((C.RiscoSeguranca, [C.Seguranca.RiscoChoque, C.Seguranca.RiscoCirculacao])), Config());
        Assert.Equal(60, soma.Itens.Single(i => i.CriterioCodigo == C.RiscoSeguranca).Pontos);

        var criterios = Criterios();
        criterios[0] = criterios[0] with { Agregacao = AgregacaoCriterio.Maximo };
        var maximo = MotorPriorizacao.Classificar(Fatos((C.RiscoSeguranca, [C.Seguranca.RiscoChoque, C.Seguranca.RiscoCirculacao])), Config(criterios));
        Assert.Equal(40, maximo.Itens.Single(i => i.CriterioCodigo == C.RiscoSeguranca).Pontos);
    }

    [Fact]
    public void Fato_ausente_vira_a_opcao_desconhecida_e_nao_risco_zero()
    {
        var r = MotorPriorizacao.Classificar(Fatos(), Config());

        var pessoas = r.Itens.Single(i => i.CriterioCodigo == C.PessoasAfetadas);
        Assert.Equal([C.Faixa.Desconhecida], pessoas.OpcoesCodigos);
        Assert.Equal(15, pessoas.Pontos);
        Assert.True(pessoas.RequerConfirmacao);
        Assert.Equal(15, r.Itens.Single(i => i.CriterioCodigo == C.RiscoSeguranca).Pontos);
    }

    [Fact]
    public void Opcao_sem_pontos_definidos_e_sinalizada_e_nao_soma()
    {
        var criterios = Criterios();
        criterios[1] = criterios[1] with
        {
            Opcoes = [new(C.Faixa.Ate10, "Até 10", null, false, false), new(C.Faixa.Desconhecida, "Desconhecida", 15, true, true)],
        };
        var r = MotorPriorizacao.Classificar(Fatos((C.PessoasAfetadas, [C.Faixa.Ate10])), Config(criterios));
        var item = r.Itens.Single(i => i.CriterioCodigo == C.PessoasAfetadas);

        Assert.True(item.SemPontosDefinidos);
        Assert.True(item.RequerConfirmacao);
        Assert.Equal(0, item.Pontos);
    }

    [Fact]
    public void Criterio_desabilitado_nao_entra_no_calculo()
    {
        var criterios = Criterios();
        criterios[1] = criterios[1] with { Habilitado = false };
        var r = MotorPriorizacao.Classificar(Fatos((C.PessoasAfetadas, [C.Faixa.MilOuMais])), Config(criterios));

        Assert.DoesNotContain(r.Itens, i => i.CriterioCodigo == C.PessoasAfetadas);
    }

    [Fact]
    public void Regra_de_precedencia_eleva_a_prioridade_acima_da_faixa_e_registra_nivel()
    {
        var regra = new RegraConfig("RISCO_VIDA", "Risco à vida", [new CondicaoRegra(C.RiscoSeguranca, [C.Seguranca.RiscoChoque])], 3, 1);
        var r = MotorPriorizacao.Classificar(
            Fatos((C.RiscoSeguranca, [C.Seguranca.RiscoChoque]), (C.PessoasAfetadas, [C.Faixa.Ate10]), (C.TipoManutencao, [C.Manutencao.Preventiva])),
            Config(regras: [regra]));

        Assert.Equal(45, r.Pontuacao);
        Assert.Equal("BAI", r.PrioridadePelaFaixa.Codigo);
        Assert.Equal("URG", r.Prioridade.Codigo);
        Assert.Equal(3, r.NivelPrecedencia);
        Assert.Contains("Risco à vida", r.MotivoPrincipal);
        Assert.Single(r.RegrasAplicadas);
    }

    [Fact]
    public void Muitos_pontos_nao_neutralizam_risco_critico()
    {
        // Ocorrência comum com muita pontuação × ocorrência com risco à vida e poucos pontos.
        var regra = new RegraConfig("RISCO_VIDA", "Risco à vida", [new CondicaoRegra(C.RiscoSeguranca, [C.Seguranca.RiscoChoque])], 3, 1);
        var config = Config(regras: [regra], faixas: [new FaixaConfig(3, 0, 1000), new FaixaConfig(1, 1001, null)]);
        var comum = MotorPriorizacao.Classificar(Fatos((C.PessoasAfetadas, [C.Faixa.MilOuMais]), (C.RiscoSeguranca, [C.Seguranca.RiscoCirculacao])), config);
        var critica = MotorPriorizacao.Classificar(Fatos((C.RiscoSeguranca, [C.Seguranca.RiscoChoque]), (C.PessoasAfetadas, [C.Faixa.Ate10])), config);

        Assert.True(comum.Pontuacao > critica.Pontuacao);
        var ordem = new[] { Chave(comum, "A"), Chave(critica, "B") }.Order(PoliticaOrdenacao.Instancia).ToList();
        Assert.Equal("B", ordem[0].Numero);
    }

    [Fact]
    public void Regra_so_se_aplica_quando_todas_as_condicoes_sao_atendidas()
    {
        var regra = new RegraConfig("PREV", "Preventiva com prazo", [
            new CondicaoRegra(C.TipoManutencao, [C.Manutencao.Preventiva]),
            new CondicaoRegra(C.ProximidadePrazo, [C.Prazo.Menos24h]),
        ], 1, 2);
        var semPrazo = MotorPriorizacao.Classificar(Fatos((C.TipoManutencao, [C.Manutencao.Preventiva]), (C.ProximidadePrazo, [C.Prazo.Mais72h])), Config(regras: [regra]));
        var comPrazo = MotorPriorizacao.Classificar(Fatos((C.TipoManutencao, [C.Manutencao.Preventiva]), (C.ProximidadePrazo, [C.Prazo.Menos24h])), Config(regras: [regra]));

        Assert.Empty(semPrazo.RegrasAplicadas);
        Assert.Equal("ALT", comPrazo.Prioridade.Codigo);
    }

    [Theory]
    [InlineData(0, "BAI")]
    [InlineData(49, "BAI")]
    [InlineData(50, "ALT")]
    [InlineData(99, "ALT")]
    [InlineData(100, "URG")]
    [InlineData(5000, "URG")]
    public void Faixas_enquadram_nos_limites(int pontos, string esperado)
    {
        var criterio = new CriterioConfig("X", "X", AgregacaoCriterio.Maximo, true, [new("V", "V", pontos, false, false)]);
        var r = MotorPriorizacao.Classificar(Fatos(("X", ["V"])), Config([criterio]));
        Assert.Equal(esperado, r.Prioridade.Codigo);
    }

    private static ChaveFila Chave(ResultadoClassificacao r, string numero) =>
        new(Guid.NewGuid(), 1, r.NivelPrecedencia, r.Prioridade.Rank, TipoManutencao.Corretiva, r.Pontuacao, null, DateTimeOffset.UnixEpoch, numero);
}

public class ExtratorFatosTests
{
    private static OrdemServico Os(DateTimeOffset abertura) => new() { Numero = "X", AbertaEm = abertura };

    [Fact]
    public void Os_sem_fatos_informados_recebe_opcoes_desconhecidas()
    {
        var agora = DateTimeOffset.UtcNow;
        var f = ExtratorFatos.Extrair(Os(agora), new ContextoFatos("OUTROS", [], null, new Dictionary<string, IReadOnlyList<string>>()), agora);

        Assert.Equal([C.Seguranca.Desconhecida], f.Respostas[C.RiscoSeguranca]);
        Assert.Equal([C.Faixa.Desconhecida], f.Respostas[C.PessoasAfetadas]);
        Assert.Equal([C.ClasseNaoIdentificada], f.Respostas[C.ClasseCliente]);
        Assert.Equal([C.UcsNoTransformador.NaoIdentificado], f.Respostas[C.UcsTransformador]);
        Assert.Equal([C.Duracao.Desconhecida], f.Respostas[C.DuracaoInterrupcao]);
        Assert.Equal([C.Situacao.NaoInformada], f.Respostas[C.SituacaoCliente]);
    }

    [Theory]
    [InlineData(0.5, C.Espera.Ate1h)]
    [InlineData(3, C.Espera.De1A4h)]
    [InlineData(10, C.Espera.De4A12h)]
    [InlineData(20, C.Espera.De12A24h)]
    [InlineData(30, C.Espera.Acima24h)]
    public void Tempo_de_espera_e_calculado_pela_abertura(double horas, string esperado)
    {
        var agora = DateTimeOffset.UtcNow;
        var f = ExtratorFatos.Extrair(Os(agora.AddHours(-horas)), new ContextoFatos("OUTROS", [], null, new Dictionary<string, IReadOnlyList<string>>()), agora);
        Assert.Equal([esperado], f.Respostas[C.TempoEspera]);
    }

    [Fact]
    public void Proximidade_do_prazo_considera_vencimento()
    {
        var agora = DateTimeOffset.UtcNow;
        Assert.Equal(C.Prazo.Vencido, C.Prazo.DeLimite(agora.AddMinutes(-1), agora));
        Assert.Equal(C.Prazo.Menos24h, C.Prazo.DeLimite(agora.AddHours(5), agora));
        Assert.Equal(C.Prazo.De24A72h, C.Prazo.DeLimite(agora.AddHours(30), agora));
        Assert.Equal(C.Prazo.Mais72h, C.Prazo.DeLimite(agora.AddDays(5), agora));
        Assert.Equal(C.Prazo.SemPrazoDefinido, C.Prazo.DeLimite(null, agora));
    }
}

public class FundirFatosTests
{
    [Fact]
    public void Novo_relato_completa_desconhecidos_mantem_conhecidos_e_une_riscos()
    {
        var destino = new OrdemServico
        {
            Numero = "A", PessoasAfetadas = C.Faixa.De11A100, ServicoEssencial = C.Essencial.NaoIdentificado, CondicaoFornecimento = C.Fornecimento.Parcial,
            CondicoesSeguranca = [new OsCondicaoSeguranca { Codigo = C.Seguranca.SemRiscoAdicional }],
        };
        var relato = new OrdemServico
        {
            Numero = "B", PessoasAfetadas = C.Faixa.De101A500, ServicoEssencial = C.Essencial.Hospital, CondicaoFornecimento = C.Fornecimento.Total,
            CondicoesSeguranca = [new OsCondicaoSeguranca { Codigo = C.Seguranca.CaboEnergizado }],
        };

        SolicitacaoService.FundirFatos(destino, relato);

        Assert.Equal(C.Faixa.De101A500, destino.PessoasAfetadas);
        Assert.Equal(C.Essencial.Hospital, destino.ServicoEssencial);
        Assert.Equal(C.Fornecimento.Parcial, destino.CondicaoFornecimento);
        Assert.Equal([C.Seguranca.CaboEnergizado], destino.CondicoesSeguranca.Select(c => c.Codigo));
    }
}
