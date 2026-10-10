namespace Farol.Domain.Rules;

/// <summary>
/// Códigos dos critérios fixos da versão 1 (seção 7.1) e de suas opções.
/// Os códigos são a ligação estável entre os fatos da OS, os pontos definidos pelo
/// Usuário Chave e as regras de precedência. Nenhum valor de pontos vive aqui.
/// </summary>
public static class Criterios
{
    public const string RiscoSeguranca = "RISCO_SEGURANCA";
    public const string ServicoEssencial = "SERVICO_ESSENCIAL";
    public const string FonteReserva = "FONTE_RESERVA";
    public const string PessoasAfetadas = "PESSOAS_AFETADAS";
    public const string UcsAfetadas = "UCS_AFETADAS";
    public const string CondicaoFornecimento = "CONDICAO_FORNECIMENTO";
    public const string DuracaoInterrupcao = "DURACAO_INTERRUPCAO";
    public const string TipoOcorrencia = "TIPO_OCORRENCIA";
    public const string EquipamentoAfetado = "EQUIPAMENTO_AFETADO";
    public const string Abrangencia = "ABRANGENCIA";
    public const string NivelRede = "NIVEL_REDE";
    public const string UcsTransformador = "UCS_TRANSFORMADOR";
    public const string TempoEspera = "TEMPO_ESPERA";
    public const string ProximidadePrazo = "PROXIMIDADE_PRAZO";
    public const string TipoManutencao = "TIPO_MANUTENCAO";
    public const string Redundancia = "REDUNDANCIA";
    public const string EquipeEspecializada = "EQUIPE_ESPECIALIZADA";
    public const string ClasseCliente = "CLASSE_CLIENTE";
    public const string SituacaoCliente = "SITUACAO_CLIENTE";

    /// <summary>Critérios cujas opções vêm de um cadastro (tipos de ocorrência, classes).</summary>
    public static readonly IReadOnlySet<string> ComOpcoesDeCadastro = new HashSet<string> { TipoOcorrencia, ClasseCliente };

    public static class Seguranca
    {
        public const string RiscoChoque = "RISCO_CHOQUE";
        public const string CaboEnergizado = "CABO_ENERGIZADO";
        public const string Incendio = "INCENDIO";
        public const string EstruturaQueda = "ESTRUTURA_QUEDA";
        public const string RiscoCirculacao = "RISCO_CIRCULACAO";
        public const string SemRiscoAdicional = "SEM_RISCO_ADICIONAL";
        public const string Desconhecida = "DESCONHECIDA";

        /// <summary>Opções que não podem ser combinadas com nenhuma outra.</summary>
        public static readonly IReadOnlySet<string> Exclusivas = new HashSet<string> { SemRiscoAdicional, Desconhecida };
    }

    public static class Essencial
    {
        public const string Hospital = "HOSPITAL";
        public const string Emergencia = "EMERGENCIA";
        public const string InfraestruturaCritica = "INFRAESTRUTURA_CRITICA";
        public const string Outro = "OUTRO";
        public const string NaoIdentificado = "NAO_IDENTIFICADO";
    }

    /// <summary>Opções de três vias: confirmado, negado ou não se sabe.</summary>
    public static class Confirmacao
    {
        public const string Sim = "SIM";
        public const string Nao = "NAO";
        public const string Desconhecida = "DESCONHECIDA";
    }

    public static class Faixa
    {
        public const string Ate10 = "ATE_10";
        public const string De11A100 = "DE_11_A_100";
        public const string De101A500 = "DE_101_A_500";
        public const string De501A999 = "DE_501_A_999";
        public const string MilOuMais = "MIL_OU_MAIS";
        public const string Desconhecida = "DESCONHECIDA";

        public static readonly string[] Todas = [Ate10, De11A100, De101A500, De501A999, MilOuMais, Desconhecida];

        public static string DeQuantidade(int quantidade) => quantidade switch
        {
            <= 10 => Ate10,
            <= 100 => De11A100,
            <= 500 => De101A500,
            <= 999 => De501A999,
            _ => MilOuMais,
        };
    }

    public static class Fornecimento
    {
        public const string SemInterrupcao = "SEM_INTERRUPCAO";
        public const string Parcial = "PARCIAL";
        public const string Total = "TOTAL";
        public const string Desconhecida = "DESCONHECIDA";
    }

    public static class Duracao
    {
        public const string Ate1h = "ATE_1H";
        public const string De1A4h = "DE_1_A_4H";
        public const string De4A12h = "DE_4_A_12H";
        public const string Acima12h = "ACIMA_12H";
        public const string Desconhecida = "DESCONHECIDA";

        public static string DeMinutos(int? minutos) => minutos switch
        {
            null => Desconhecida,
            <= 60 => Ate1h,
            <= 240 => De1A4h,
            <= 720 => De4A12h,
            _ => Acima12h,
        };
    }

    public static class Equipamento
    {
        public const string Transformador = "TRANSFORMADOR";
        public const string Poste = "POSTE";
        public const string Cabo = "CABO";
        public const string Chave = "CHAVE";
        public const string Outro = "OUTRO";
        public const string NaoIdentificado = "NAO_IDENTIFICADO";
    }

    public static class Abrange
    {
        public const string PontoUnico = "PONTO_UNICO";
        public const string RuaTrecho = "RUA_TRECHO";
        public const string Bairro = "BAIRRO";
        public const string MultiplosBairros = "MULTIPLOS_BAIRROS";
        public const string Desconhecida = "DESCONHECIDA";
    }

    public static class Rede
    {
        public const string Circuito = "CIRCUITO";
        public const string Conjunto = "CONJUNTO";
        public const string Transformador = "TRANSFORMADOR";
        public const string RamalUc = "RAMAL_UC";
        public const string NaoIdentificado = "NAO_IDENTIFICADO";
    }

    public static class UcsNoTransformador
    {
        public const string Ate20 = "ATE_20";
        public const string De21A50 = "DE_21_A_50";
        public const string Acima50 = "ACIMA_50";
        public const string NaoIdentificado = "NAO_IDENTIFICADO";

        public static string DeQuantidade(int? quantidade) => quantidade switch
        {
            null => NaoIdentificado,
            <= 20 => Ate20,
            <= 50 => De21A50,
            _ => Acima50,
        };
    }

    public static class Espera
    {
        public const string Ate1h = "ATE_1H";
        public const string De1A4h = "DE_1_A_4H";
        public const string De4A12h = "DE_4_A_12H";
        public const string De12A24h = "DE_12_A_24H";
        public const string Acima24h = "ACIMA_24H";

        public static string DeDuracao(TimeSpan espera) => espera.TotalHours switch
        {
            <= 1 => Ate1h,
            <= 4 => De1A4h,
            <= 12 => De4A12h,
            <= 24 => De12A24h,
            _ => Acima24h,
        };
    }

    public static class Prazo
    {
        public const string SemPrazoDefinido = "SEM_PRAZO_DEFINIDO";
        public const string Mais72h = "MAIS_72H";
        public const string De24A72h = "DE_24_A_72H";
        public const string Menos24h = "MENOS_24H";
        public const string Vencido = "VENCIDO";

        public static string DeLimite(DateTimeOffset? limite, DateTimeOffset agora)
        {
            if (limite is null) return SemPrazoDefinido;
            var restante = limite.Value - agora;
            if (restante <= TimeSpan.Zero) return Vencido;
            if (restante.TotalHours < 24) return Menos24h;
            return restante.TotalHours <= 72 ? De24A72h : Mais72h;
        }
    }

    public static class Manutencao
    {
        public const string Corretiva = "CORRETIVA";
        public const string Preventiva = "PREVENTIVA";
    }

    public static class Situacao
    {
        public const string Ligado = "LIGADO";
        public const string Desligado = "DESLIGADO";
        public const string NaoInformada = "NAO_INFORMADA";
    }

    public const string ClasseNaoIdentificada = "NAO_IDENTIFICADA";
}
