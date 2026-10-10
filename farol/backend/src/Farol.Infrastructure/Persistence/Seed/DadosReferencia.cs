using Farol.Domain.Entities;
using Farol.Domain.Enums;
using C = Farol.Domain.Rules.Criterios;

namespace Farol.Infrastructure.Persistence.Seed;

/// <summary>
/// Estrutura de referência da versão 1 (critérios fixos e suas opções) e o conjunto DEMONSTRATIVO de pontos,
/// faixas, prazos e regras. Nada aqui é regra oficial do SGM: tudo nasce marcado como demonstrativo
/// e é substituído pelo Usuário Chave (pontos e faixas) e por administradores (prazos e regras).
/// </summary>
public static class DadosReferencia
{
    public sealed record OpcaoSemente(string Codigo, string Rotulo, int PontosDemo, bool Desconhecido = false, bool RequerConfirmacao = false);

    public sealed record CriterioSemente(
        string Codigo, string Nome, string Descricao, string Regra, AgregacaoCriterio Agregacao, bool Multipla, OpcaoSemente[] Opcoes);

    public static readonly CriterioSemente[] Criterios =
    [
        new(C.RiscoSeguranca, "Risco à vida e à segurança", "Condições de segurança observadas: choque, cabo energizado, incêndio, estrutura instável, circulação.",
            "Condições marcadas no formulário; várias condições somam pontos.", AgregacaoCriterio.Soma, true,
        [
            new(C.Seguranca.RiscoChoque, "Risco de choque elétrico", 60),
            new(C.Seguranca.CaboEnergizado, "Cabo energizado exposto ou caído", 60),
            new(C.Seguranca.Incendio, "Incêndio ou risco de incêndio", 60),
            new(C.Seguranca.EstruturaQueda, "Poste ou estrutura com risco de queda", 40),
            new(C.Seguranca.RiscoCirculacao, "Risco à circulação de pessoas ou veículos", 25),
            new(C.Seguranca.SemRiscoAdicional, "Sem risco adicional identificado", 0),
            new(C.Seguranca.Desconhecida, "Situação desconhecida", 15, true, true),
        ]),
        new(C.ServicoEssencial, "Serviço essencial afetado", "Hospitais, emergência, infraestrutura crítica.",
            "Detalhamento informado quando a classe é Essencial ou quando o atendente identifica o serviço.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Essencial.Hospital, "Hospital ou serviço de saúde essencial", 50),
            new(C.Essencial.Emergencia, "Serviço de emergência ou segurança pública", 45),
            new(C.Essencial.InfraestruturaCritica, "Infraestrutura crítica", 35),
            new(C.Essencial.Outro, "Outro serviço essencial", 20),
            new(C.Essencial.NaoIdentificado, "Não identificado", 5, true),
        ]),
        new(C.FonteReserva, "Fonte de energia de reserva no local crítico", "Existência de fonte de reserva confirmada.",
            "Resposta do formulário (confirmada, não, desconhecida).", AgregacaoCriterio.Maximo, false,
        [
            new(C.Confirmacao.Sim, "Confirmada", 0),
            new(C.Confirmacao.Nao, "Não há", 10),
            new(C.Confirmacao.Desconhecida, "Desconhecida", 5, true, true),
        ]),
        new(C.PessoasAfetadas, "Quantidade estimada de pessoas afetadas", "Faixa de pessoas afetadas, distinta de unidades consumidoras.",
            "Faixa selecionada; a quantidade exata, quando informada, precisa corresponder à faixa.", AgregacaoCriterio.Maximo, false, Faixas(5, 15, 25, 35, 45, 15)),
        new(C.UcsAfetadas, "Quantidade de unidades consumidoras afetadas", "Faixa de UCs afetadas.",
            "Faixa selecionada; a quantidade exata, quando informada, precisa corresponder à faixa.", AgregacaoCriterio.Maximo, false, Faixas(5, 12, 20, 28, 36, 12)),
        new(C.CondicaoFornecimento, "Extensão da interrupção", "Condição de fornecimento observada na rede.",
            "Resposta do formulário.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Fornecimento.SemInterrupcao, "Sem interrupção", 0),
            new(C.Fornecimento.Parcial, "Interrupção parcial", 15),
            new(C.Fornecimento.Total, "Interrupção total", 25),
            new(C.Fornecimento.Desconhecida, "Interrupção desconhecida", 15, true, true),
        ]),
        new(C.DuracaoInterrupcao, "Duração estimada da interrupção", "Faixa derivada da duração estimada informada.",
            "Derivada do campo de duração (minutos); sem valor = desconhecida.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Duracao.Ate1h, "Até 1 hora", 0),
            new(C.Duracao.De1A4h, "De 1 a 4 horas", 5),
            new(C.Duracao.De4A12h, "De 4 a 12 horas", 10),
            new(C.Duracao.Acima12h, "Acima de 12 horas", 15),
            new(C.Duracao.Desconhecida, "Desconhecida", 5, true),
        ]),
        new(C.TipoOcorrencia, "Tipo e gravidade da ocorrência", "Opções sincronizadas com o cadastro de tipos de ocorrência.",
            "Tipo selecionado no formulário. Novos tipos nascem sem pontos.", AgregacaoCriterio.Maximo, false, []),
        new(C.EquipamentoAfetado, "Equipamento afetado", "Equipamento envolvido na ocorrência.",
            "Resposta do formulário.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Equipamento.Transformador, "Transformador", 15),
            new(C.Equipamento.Chave, "Chave ou equipamento de manobra", 12),
            new(C.Equipamento.Cabo, "Cabo / condutor", 12),
            new(C.Equipamento.Poste, "Poste ou estrutura", 10),
            new(C.Equipamento.Outro, "Outro equipamento", 5),
            new(C.Equipamento.NaoIdentificado, "Não identificado", 5, true),
        ]),
        new(C.Abrangencia, "Abrangência geográfica", "Extensão territorial afetada.",
            "Resposta do formulário.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Abrange.PontoUnico, "Ponto único", 0),
            new(C.Abrange.RuaTrecho, "Rua ou trecho", 5),
            new(C.Abrange.Bairro, "Bairro", 10),
            new(C.Abrange.MultiplosBairros, "Múltiplos bairros", 15),
            new(C.Abrange.Desconhecida, "Desconhecida", 5, true),
        ]),
        new(C.NivelRede, "Circuito e conjunto afetados", "Nível da rede elétrica atingido: circuito (subestação), conjunto, transformador ou ramal.",
            "Resposta do formulário, confirmada pelo cadastro quando houver.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Rede.Circuito, "Circuito inteiro (subestação)", 20),
            new(C.Rede.Conjunto, "Conjunto elétrico", 15),
            new(C.Rede.Transformador, "Transformador", 10),
            new(C.Rede.RamalUc, "Ramal / unidade consumidora", 2),
            new(C.Rede.NaoIdentificado, "Não identificado", 5, true),
        ]),
        new(C.UcsTransformador, "UCs ligadas ao transformador", "Quantidade de UCs ligadas ao transformador no cadastro.",
            "Contagem no cadastro de UCs; transformador fora do cadastro = não identificado.", AgregacaoCriterio.Maximo, false,
        [
            new(C.UcsNoTransformador.Ate20, "Até 20 UCs", 2),
            new(C.UcsNoTransformador.De21A50, "De 21 a 50 UCs", 5),
            new(C.UcsNoTransformador.Acima50, "Acima de 50 UCs", 8),
            new(C.UcsNoTransformador.NaoIdentificado, "Transformador não identificado no cadastro", 3, true),
        ]),
        new(C.TempoEspera, "Tempo decorrido desde a abertura", "Faixa de espera desde a abertura da OS.",
            "Calculado pelo servidor; a OS é reclassificada automaticamente ao mudar de faixa.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Espera.Ate1h, "Até 1 hora", 0),
            new(C.Espera.De1A4h, "De 1 a 4 horas", 3),
            new(C.Espera.De4A12h, "De 4 a 12 horas", 6),
            new(C.Espera.De12A24h, "De 12 a 24 horas", 9),
            new(C.Espera.Acima24h, "Acima de 24 horas", 12),
        ]),
        new(C.ProximidadePrazo, "Proximidade do prazo-limite", "Proximidade da data e horário-limite operacional informado.",
            "Calculado pelo servidor a partir da data-limite; reclassificação automática.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Prazo.SemPrazoDefinido, "Sem prazo-limite definido", 0),
            new(C.Prazo.Mais72h, "Mais de 72 horas", 0),
            new(C.Prazo.De24A72h, "De 24 a 72 horas", 5),
            new(C.Prazo.Menos24h, "Menos de 24 horas", 10),
            new(C.Prazo.Vencido, "Vencido", 15),
        ]),
        new(C.TipoManutencao, "Tipo de manutenção", "Corretiva ou preventiva.",
            "Campo obrigatório do formulário.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Manutencao.Corretiva, "Corretiva", 10),
            new(C.Manutencao.Preventiva, "Preventiva", 0),
        ]),
        new(C.Redundancia, "Redundância ou alimentação alternativa", "Existência de redundância confirmada.",
            "Resposta do formulário.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Confirmacao.Sim, "Confirmada", 0),
            new(C.Confirmacao.Nao, "Não há", 8),
            new(C.Confirmacao.Desconhecida, "Desconhecida", 5, true),
        ]),
        new(C.EquipeEspecializada, "Necessidade de equipe especializada", "Se o serviço exige equipe especializada.",
            "Resposta do formulário; \"sim\" também restringe as equipes compatíveis.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Confirmacao.Sim, "Sim", 3),
            new(C.Confirmacao.Nao, "Não", 0),
            new(C.Confirmacao.Desconhecida, "Desconhecida", 1, true),
        ]),
        new(C.ClasseCliente, "Classe do cliente", "Opções sincronizadas com o cadastro de classes; com várias classes, vale a maior.",
            "Classe confirmada pelo atendente (vem do cadastro da UC quando identificada).", AgregacaoCriterio.Maximo, true, []),
        new(C.SituacaoCliente, "Situação do cliente", "Unidade consumidora ligada ou desligada no momento do registro.",
            "Resposta do formulário; \"não informada\" não é tratada como baixo risco.", AgregacaoCriterio.Maximo, false,
        [
            new(C.Situacao.Ligado, "Ligado", 5),
            new(C.Situacao.Desligado, "Desligado", 0),
            new(C.Situacao.NaoInformada, "Não informada", 3, true, true),
        ]),
    ];

    private static OpcaoSemente[] Faixas(int a, int b, int c, int d, int e, int desconhecida) =>
    [
        new(C.Faixa.Ate10, "Até 10", a),
        new(C.Faixa.De11A100, "De 11 a 100", b),
        new(C.Faixa.De101A500, "De 101 a 500", c),
        new(C.Faixa.De501A999, "De 501 a 999", d),
        new(C.Faixa.MilOuMais, "1.000 ou mais", e),
        new(C.Faixa.Desconhecida, "Desconhecida", desconhecida, true, true),
    ];

    public static readonly (string Codigo, string Nome)[] Qualificacoes =
    [
        ("NR10", "Segurança em instalações elétricas (NR-10)"),
        ("SEP", "Sistema Elétrico de Potência (SEP)"),
        ("TRABALHO_ALTURA", "Trabalho em altura (NR-35)"),
        ("LINHA_VIVA", "Linha viva"),
        ("PODA", "Poda próxima à rede"),
        ("OPERACAO_MANOBRA", "Operação e manobra de chaves"),
        ("EQUIPE_ESPECIALIZADA", "Equipe especializada"),
    ];

    public static readonly (string Codigo, string Nome)[] Recursos =
    [
        ("CAMINHAO_CESTO", "Caminhão com cesto aéreo"),
        ("GUINDAUTO", "Guindauto"),
        ("MOTOSSERRA", "Motosserra"),
        ("GERADOR", "Gerador portátil"),
        ("TRANSFORMADOR_RESERVA", "Transformador de reserva"),
        ("KIT_ATERRAMENTO", "Kit de aterramento temporário"),
    ];

    public sealed record TipoSemente(string Codigo, string Nome, TipoManutencao? Sugerido, int PontosDemo, string[] Qualificacoes);

    public static readonly TipoSemente[] Tipos =
    [
        new("INTERRUPCAO_TOTAL", "Interrupção total de energia", TipoManutencao.Corretiva, 20, ["NR10", "SEP"]),
        new("INTERRUPCAO_PARCIAL", "Interrupção parcial de energia", TipoManutencao.Corretiva, 12, ["NR10", "SEP"]),
        new("TRANSFORMADOR_DANIFICADO", "Transformador danificado", TipoManutencao.Corretiva, 20, ["NR10", "SEP", "TRABALHO_ALTURA"]),
        new("GALHO_NA_REDE", "Galho ou objeto em contato com a rede", TipoManutencao.Corretiva, 12, ["NR10", "PODA"]),
        new("CABO_ROMPIDO", "Cabo rompido ou caído", TipoManutencao.Corretiva, 30, ["NR10", "SEP"]),
        new("POSTE_DANIFICADO", "Poste danificado", TipoManutencao.Corretiva, 20, ["NR10", "TRABALHO_ALTURA"]),
        new("FALHA_EQUIPAMENTO", "Equipamento elétrico com falha", TipoManutencao.Corretiva, 15, ["NR10", "SEP"]),
        new("CURTO_INCENDIO", "Curto-circuito ou princípio de incêndio", TipoManutencao.Corretiva, 30, ["NR10", "SEP"]),
        new("RISCO_CHOQUE", "Risco de choque elétrico", TipoManutencao.Corretiva, 30, ["NR10", "SEP"]),
        new("FALHA_CHAVE", "Falha em chave ou equipamento de manobra", TipoManutencao.Corretiva, 15, ["NR10", "OPERACAO_MANOBRA"]),
        new("MANUTENCAO_PROGRAMADA", "Manutenção programada", TipoManutencao.Preventiva, 0, ["NR10"]),
        new("INSPECAO_PREVENTIVA", "Inspeção preventiva", TipoManutencao.Preventiva, 0, ["NR10"]),
        new("OUTROS", "Outros", null, 5, ["NR10"]),
    ];

    public sealed record ClasseSemente(string Codigo, string Nome, bool Essencial, int PontosDemo);

    /// <summary>As quatro classes iniciais da operação e duas cadastradas para acomodar a base fictícia.</summary>
    public static readonly ClasseSemente[] Classes =
    [
        new("PODER_PUBLICO", "Poder público", false, 20),
        new("RURAL", "Rural", false, 10),
        new("ESSENCIAL", "Essencial", true, 30),
        new("RESIDENCIAL", "Residencial", false, 5),
        new("COMERCIAL", "Comercial", false, 8),
        new("INDUSTRIAL", "Industrial", false, 10),
    ];

    public const int PontosClasseNaoIdentificada = 5;

    public sealed record PrioridadeSemente(string Codigo, string Nome, string Descricao, int Rank, string Cor, bool Critica,
        int Triagem, int Despacho, int Inicio, int? Restabelecimento, int Conclusao, CalendarioPrazo Calendario, int Minimo, int? Maximo);

    /// <summary>Códigos, prazos (minutos) e faixas demonstrativos — sem correspondência com a tabela oficial do SGM.</summary>
    public static readonly PrioridadeSemente[] Prioridades =
    [
        new("URG", "Urgente", "Risco iminente à vida ou à segurança pública.", 1, "#B42318", true, 10, 20, 60, 240, 480, CalendarioPrazo.Corrido, 220, null),
        new("EMG", "Emergente", "Impacto severo em serviço essencial ou grande número de clientes.", 2, "#C4320A", true, 15, 40, 120, 360, 720, CalendarioPrazo.Corrido, 160, 219),
        new("ALT", "Alta", "Interrupção relevante ou risco controlado.", 3, "#B54708", false, 30, 120, 360, 720, 1440, CalendarioPrazo.Corrido, 110, 159),
        new("MED", "Média", "Interrupção localizada sem risco adicional.", 4, "#2457A6", false, 60, 480, 1440, null, 4320, CalendarioPrazo.Util, 60, 109),
        new("BAI", "Baixa", "Serviços programáveis e preventivos sem urgência.", 5, "#667A45", false, 120, 1440, 4320, null, 10080, CalendarioPrazo.Util, 0, 59),
    ];

    public sealed record RegraSemente(string Codigo, string Nome, string Descricao, CondicaoRegra[] Condicoes, int Nivel, string PrioridadeMinima);

    public static readonly RegraSemente[] Regras =
    [
        new("RISCO_VIDA", "Risco iminente à vida", "Choque elétrico, cabo energizado ou incêndio sobem a OS para Urgente, à frente de qualquer pontuação.",
            [new CondicaoRegra(C.RiscoSeguranca, [C.Seguranca.RiscoChoque, C.Seguranca.CaboEnergizado, C.Seguranca.Incendio])], 3, "URG"),
        new("ESSENCIAL_SEM_RESERVA", "Serviço essencial sem reserva", "Hospital ou emergência com interrupção e sem fonte de reserva confirmada.",
            [
                new CondicaoRegra(C.ServicoEssencial, [C.Essencial.Hospital, C.Essencial.Emergencia]),
                new CondicaoRegra(C.CondicaoFornecimento, [C.Fornecimento.Parcial, C.Fornecimento.Total, C.Fornecimento.Desconhecida]),
                new CondicaoRegra(C.FonteReserva, [C.Confirmacao.Nao, C.Confirmacao.Desconhecida]),
            ], 2, "EMG"),
        new("ESTRUTURA_INSTAVEL", "Estrutura com risco de queda", "Poste ou estrutura com risco de queda é tratado no mínimo como Alta.",
            [new CondicaoRegra(C.RiscoSeguranca, [C.Seguranca.EstruturaQueda])], 1, "ALT"),
        new("PREVENTIVA_PRAZO_IMINENTE", "Preventiva com prazo iminente", "Preventiva com prazo-limite em menos de 24 h (ou vencido) não fica atrás de corretivas comuns.",
            [
                new CondicaoRegra(C.TipoManutencao, [C.Manutencao.Preventiva]),
                new CondicaoRegra(C.ProximidadePrazo, [C.Prazo.Menos24h, C.Prazo.Vencido]),
            ], 1, "ALT"),
    ];

    /// <summary>Feriados nacionais de data fixa (calendário útil). Feriados móveis e municipais são cadastrados pela operação.</summary>
    public static IEnumerable<Feriado> FeriadosNacionais(int anoInicial, int anos)
    {
        var fixos = new (int Mes, int Dia, string Nome)[]
        {
            (1, 1, "Confraternização Universal"), (4, 21, "Tiradentes"), (5, 1, "Dia do Trabalho"), (9, 7, "Independência do Brasil"),
            (10, 12, "Nossa Senhora Aparecida"), (11, 2, "Finados"), (11, 15, "Proclamação da República"), (11, 20, "Dia da Consciência Negra"),
            (12, 25, "Natal"),
        };
        for (var ano = anoInicial; ano < anoInicial + anos; ano++)
            foreach (var (mes, dia, nome) in fixos)
                yield return new Feriado { Data = new DateOnly(ano, mes, dia), Nome = nome };
    }
}
