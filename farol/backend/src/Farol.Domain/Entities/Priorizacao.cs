using Farol.Domain.Enums;

namespace Farol.Domain.Entities;

/// <summary>Critério de classificação (seção 7.1). A estrutura é do administrador; os pontos, do Usuário Chave.</summary>
public class Criterio
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public TipoCriterio Tipo { get; set; }
    public AgregacaoCriterio Agregacao { get; set; } = AgregacaoCriterio.Maximo;
    public bool MultiplaEscolha { get; set; }
    /// <summary>Texto da regra de aplicação: de onde vem o fato e como as opções são escolhidas.</summary>
    public string? RegraAplicacao { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public Guid? CriadoPorId { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public Guid? AlteradoPorId { get; set; }
    public DateTimeOffset AlteradoEm { get; set; }
    public List<OpcaoCriterio> Opcoes { get; set; } = [];
    /// <summary>Vazio = todas as cidades.</summary>
    public List<CriterioMunicipio> Municipios { get; set; } = [];
}

public class CriterioMunicipio
{
    public int CriterioId { get; set; }
    public int MunicipioId { get; set; }
}

public class OpcaoCriterio
{
    public int Id { get; set; }
    public int CriterioId { get; set; }
    public Criterio? Criterio { get; set; }
    public required string Codigo { get; set; }
    public required string Rotulo { get; set; }
    public int Ordem { get; set; }
    /// <summary>Opção que representa ausência de informação ("Desconhecida", "Não informada"...).</summary>
    public bool RepresentaDesconhecido { get; set; }
    public bool Ativa { get; set; } = true;
}

/// <summary>Condição de uma regra de precedência: o fato do critério está entre as opções listadas.</summary>
public record CondicaoRegra(string Criterio, string[] Opcoes);

/// <summary>Regra da camada A (seção 7.3), aprovada pela área operacional e de segurança.</summary>
public class RegraPrecedencia
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    /// <summary>Todas as condições precisam ser atendidas (E lógico).</summary>
    public List<CondicaoRegra> Condicoes { get; set; } = [];
    public int NivelPrecedencia { get; set; }
    public int PrioridadeMinimaId { get; set; }
    public Prioridade? PrioridadeMinima { get; set; }
    public bool Ativa { get; set; } = true;
    public bool Demonstrativa { get; set; }
    public int Versao { get; set; } = 1;
    public Guid? AprovadaPorId { get; set; }
    public DateTimeOffset? AprovadaEm { get; set; }
    public DateTimeOffset AlteradaEm { get; set; }
}

/// <summary>Código de prioridade com seus prazos (seção 8). As faixas de pontos ficam na versão de pontuação.</summary>
public class Prioridade
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    /// <summary>1 = mais urgente.</summary>
    public int Rank { get; set; }
    public required string Cor { get; set; }
    /// <summary>Conta no indicador "OS críticas ou urgentes".</summary>
    public bool Critica { get; set; }
    public int PrazoTriagemMin { get; set; }
    public int PrazoDespachoMin { get; set; }
    public int PrazoInicioMin { get; set; }
    public int? PrazoRestabelecimentoMin { get; set; }
    public int PrazoConclusaoMin { get; set; }
    /// <summary>Unidade em que os prazos são exibidos e editados (minutos, horas, dias).</summary>
    public string UnidadePrazo { get; set; } = "HORAS";
    public CalendarioPrazo Calendario { get; set; }
    public bool ConsideraFeriados { get; set; } = true;
    public string? TratamentoCritico { get; set; }
    public string? Escalonamento { get; set; }
    public DateTimeOffset VigenciaInicio { get; set; }
    public DateTimeOffset? VigenciaFim { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Demonstrativa { get; set; }
}

/// <summary>Conjunto de pontos e faixas publicado pelo Usuário Chave (seção 7A).</summary>
public class VersaoPontuacao
{
    public int Id { get; set; }
    /// <summary>Nulo = pontuação global.</summary>
    public int? MunicipioId { get; set; }
    public Municipio? Municipio { get; set; }
    public int Numero { get; set; }
    public StatusVersaoPontuacao Status { get; set; }
    public bool Demonstrativa { get; set; }
    public DateTimeOffset? VigenciaInicio { get; set; }
    public AplicacaoVersao? Aplicacao { get; set; }
    public string? Justificativa { get; set; }
    public Guid? AutorId { get; set; }
    public string? AutorNome { get; set; }
    public DateTimeOffset CriadaEm { get; set; }
    public DateTimeOffset AlteradaEm { get; set; }
    public Guid? PublicadaPorId { get; set; }
    public string? PublicadaPorNome { get; set; }
    public DateTimeOffset? PublicadaEm { get; set; }
    public Guid? AprovadaPorId { get; set; }
    public string? AprovadaPorNome { get; set; }
    public DateTimeOffset? AprovadaEm { get; set; }
    /// <summary>Quando a reclassificação das OS abertas desta versão já foi feita.</summary>
    public DateTimeOffset? ReclassificacaoAplicadaEm { get; set; }
    public int? RestauradaDeId { get; set; }
    public uint Versao { get; set; }

    public List<VersaoCriterio> Criterios { get; set; } = [];
    public List<PontoOpcao> Pontos { get; set; } = [];
    public List<FaixaPrioridade> Faixas { get; set; } = [];
}

public class VersaoCriterio
{
    public int VersaoPontuacaoId { get; set; }
    public int CriterioId { get; set; }
    public Criterio? Criterio { get; set; }
    public bool Habilitado { get; set; }
}

public class PontoOpcao
{
    public int VersaoPontuacaoId { get; set; }
    public int OpcaoId { get; set; }
    public OpcaoCriterio? Opcao { get; set; }
    /// <summary>Nulo = "sem pontos definidos", diferente de zero; impede a publicação.</summary>
    public int? Pontos { get; set; }
    public bool RequerConfirmacao { get; set; }
    public OrigemPontos Origem { get; set; }
    public Guid? AlteradoPorId { get; set; }
    public string? AlteradoPorNome { get; set; }
    public DateTimeOffset AlteradoEm { get; set; }
}

public class FaixaPrioridade
{
    public int VersaoPontuacaoId { get; set; }
    public int PrioridadeId { get; set; }
    public Prioridade? Prioridade { get; set; }
    public int Minimo { get; set; }
    /// <summary>Nulo = sem limite superior (faixa mais alta).</summary>
    public int? Maximo { get; set; }
}

/// <summary>Fotografia imutável de um cálculo de prioridade (Key decision 2).</summary>
public class ResultadoPrioridade
{
    public long Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public int VersaoPontuacaoId { get; set; }
    public VersaoPontuacao? VersaoPontuacao { get; set; }
    public DateTimeOffset CalculadoEm { get; set; }
    public MotivoClassificacao Motivo { get; set; }
    public int Pontuacao { get; set; }
    public int NivelPrecedencia { get; set; }
    /// <summary>Prioridade resultante do cálculo (faixa + camada A).</summary>
    public int PrioridadeCalculadaId { get; set; }
    /// <summary>Prioridade em vigor após o cálculo (difere quando há revisão manual).</summary>
    public int PrioridadeId { get; set; }
    public Prioridade? Prioridade { get; set; }
    public List<RegraAplicada> RegrasAplicadas { get; set; } = [];
    public required string MotivoPrincipal { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? Justificativa { get; set; }
    public List<ResultadoPrioridadeItem> Itens { get; set; } = [];
}

public record RegraAplicada(string Codigo, string Nome, int NivelPrecedencia, string PrioridadeMinima);

public class ResultadoPrioridadeItem
{
    public long Id { get; set; }
    public long ResultadoPrioridadeId { get; set; }
    public required string CriterioCodigo { get; set; }
    public required string CriterioNome { get; set; }
    public required string OpcoesCodigos { get; set; }
    public required string OpcoesRotulos { get; set; }
    public int Pontos { get; set; }
    public bool SemPontosDefinidos { get; set; }
    public bool RequerConfirmacao { get; set; }
}
