namespace Farol.Domain.Enums;

public enum TipoManutencao { Corretiva, Preventiva }

public enum CanalEntrada { Telefone, SistemaInterno, Presencial, Integracao }

public enum StatusOrdemServico
{
    Aberta,
    EmTriagem,
    AguardandoDespacho,
    EquipeDesignada,
    EquipeACaminho,
    EmExecucao,
    AguardandoRecurso,
    Suspensa,
    Concluida,
    Cancelada,
}

public enum StatusEquipe
{
    Disponivel,
    ACaminho,
    EmAtendimento,
    Indisponivel,
    EmPausa,
    DeslocamentoOutraAtividade,
}

/// <summary>De onde vieram as coordenadas da ocorrência (seção 4.2.2).</summary>
public enum OrigemCoordenada { Informada, Mapa, Geocodificada, Dispositivo }

/// <summary>Como circuito, conjunto e transformador chegaram à OS (seção 4.2.4).</summary>
public enum OrigemRede { NaoInformado, Informado, CadastroAConfirmar, Confirmado }

public enum OrigemLocalizacaoEquipe { Demonstrativa, Cadastrada, Gps }

public enum SituacaoUnidade { Ligado, Desligado }

public enum StatusVersaoPontuacao { Rascunho, AguardandoAprovacao, Publicada, Substituida }

public enum AplicacaoVersao { SomenteNovas, ReclassificarAbertas }

public enum OrigemPontos { Demonstrativo, UsuarioChave }

public enum MotivoClassificacao { Criacao, AtualizacaoDados, Tempo, NovaVersao, Manual, Vinculo, TrocaMunicipio }

public enum AgregacaoCriterio { Maximo, Soma }

public enum TipoCriterio { Fixo, Personalizado }

public enum CalendarioPrazo { Corrido, Util }

public enum TipoPrazo { Triagem, Despacho, Inicio, Restabelecimento, Conclusao }
