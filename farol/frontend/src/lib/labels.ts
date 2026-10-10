import type {
  AplicacaoVersao,
  CalendarioPrazo,
  CanalEntrada,
  OrigemCoordenada,
  OrigemLocalizacaoEquipe,
  OrigemPontos,
  OrigemRede,
  StatusEquipe,
  StatusOrdemServico,
  StatusVersaoPontuacao,
  TipoManutencao,
  TipoPrazo,
} from '@/types/api';

export const STATUS_OS_LABEL: Record<StatusOrdemServico, string> = {
  Aberta: 'Aberta',
  EmTriagem: 'Em triagem',
  AguardandoDespacho: 'Aguardando despacho',
  EquipeDesignada: 'Equipe designada',
  EquipeACaminho: 'Equipe a caminho',
  EmExecucao: 'Em execução',
  AguardandoRecurso: 'Aguardando recurso/autorização',
  Suspensa: 'Suspensa',
  Concluida: 'Concluída',
  Cancelada: 'Cancelada',
};

export const STATUS_EQUIPE_LABEL: Record<StatusEquipe, string> = {
  Disponivel: 'Disponível',
  ACaminho: 'A caminho',
  EmAtendimento: 'Em atendimento',
  Indisponivel: 'Indisponível',
  EmPausa: 'Em pausa',
  DeslocamentoOutraAtividade: 'Em deslocamento p/ outra atividade',
};

export const TIPO_MANUTENCAO_LABEL: Record<TipoManutencao, string> = {
  Corretiva: 'Corretiva',
  Preventiva: 'Preventiva',
};

export const CANAL_LABEL: Record<CanalEntrada, string> = {
  Telefone: 'Telefone',
  SistemaInterno: 'Sistema interno',
  Presencial: 'Atendimento presencial',
  Integracao: 'Integração',
};

export const ORIGEM_COORDENADA_LABEL: Record<OrigemCoordenada, string> = {
  Informada: 'Informada pelo solicitante',
  Mapa: 'Marcada no mapa',
  Geocodificada: 'Geocodificada (aproximada)',
  Dispositivo: 'Dispositivo no local',
};

export const ORIGEM_REDE_LABEL: Record<OrigemRede, string> = {
  NaoInformado: 'Não informado',
  Informado: 'Informado, sem confirmação no cadastro',
  CadastroAConfirmar: 'Identificado pelo cadastro, a confirmar',
  Confirmado: 'Confirmado',
};

export const ORIGEM_LOCALIZACAO_EQUIPE_LABEL: Record<OrigemLocalizacaoEquipe, string> = {
  Demonstrativa: 'Posição demonstrativa',
  Cadastrada: 'Última posição cadastrada',
  Gps: 'GPS',
};

export const ORIGEM_PONTOS_LABEL: Record<OrigemPontos, string> = {
  Demonstrativo: 'Demonstrativo',
  UsuarioChave: 'Definido pelo Usuário Chave',
};

export const TIPO_PRAZO_LABEL: Record<TipoPrazo, string> = {
  Triagem: 'Triagem',
  Despacho: 'Despacho',
  Inicio: 'Início do atendimento',
  Restabelecimento: 'Restabelecimento',
  Conclusao: 'Conclusão',
};

export const STATUS_VERSAO_LABEL: Record<StatusVersaoPontuacao, string> = {
  Rascunho: 'Rascunho',
  AguardandoAprovacao: 'Aguardando aprovação',
  Publicada: 'Publicada',
  Substituida: 'Substituída',
};

export const APLICACAO_VERSAO_LABEL: Record<AplicacaoVersao, string> = {
  SomenteNovas: 'Somente novas OS',
  ReclassificarAbertas: 'Reclassificar também as OS abertas',
};

export const CALENDARIO_LABEL: Record<CalendarioPrazo, string> = {
  Corrido: 'Corrido',
  Util: 'Dias úteis',
};

/** Faixas de quantidade (pessoas / UCs) usadas na fila. */
export const FAIXA_QUANTIDADE_LABEL: Record<string, string> = {
  ATE_10: 'Até 10',
  DE_11_A_100: '11–100',
  DE_101_A_500: '101–500',
  DE_501_A_999: '501–999',
  MIL_OU_MAIS: '1.000+',
  DESCONHECIDA: 'Desconhecida',
};

export const SITUACAO_CLIENTE_LABEL: Record<string, string> = {
  LIGADO: 'Ligado',
  DESLIGADO: 'Desligado',
  NAO_INFORMADA: 'Não informada',
};

/** Rótulos dos códigos de ação de OrdemServicoDetalheDto.acoesDisponiveis. */
export function rotuloAcao(acao: string): string {
  if (acao.startsWith('STATUS:')) {
    const st = acao.slice(7) as StatusOrdemServico;
    const destino = STATUS_OS_LABEL[st] ?? st;
    if (st === 'Cancelada') return 'Cancelar OS';
    if (st === 'Suspensa') return 'Suspender';
    return `Mudar para "${destino}"`;
  }
  const mapa: Record<string, string> = {
    DESPACHAR: 'Disponibilizar para equipe',
    ACEITE: 'Registrar aceite',
    INICIAR: 'Iniciar execução',
    CONCLUIR: 'Concluir',
    REMOVER_EQUIPE: 'Remover equipe',
    RECLASSIFICAR: 'Reclassificar prioridade',
    ATUALIZAR_FATOS: 'Atualizar fatos',
    TROCAR_MUNICIPIO: 'Trocar município',
    UNIFICAR: 'Unificar com outra OS',
    COMENTAR: 'Comentar',
  };
  return mapa[acao] ?? acao;
}

/** Status que exigem justificativa ao mudar (o servidor valida; a UI pede antes). */
export const STATUS_EXIGE_JUSTIFICATIVA: StatusOrdemServico[] = ['Cancelada', 'Suspensa', 'AguardandoDespacho', 'AguardandoRecurso'];

export function label<T extends string>(mapa: Record<T, string>, v: T | null | undefined): string {
  if (v === null || v === undefined) return '—';
  return mapa[v] ?? v;
}
