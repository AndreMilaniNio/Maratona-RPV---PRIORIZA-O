// Ponto único de importação dos tipos da API.
// Os DTOs vêm de api.generated.ts (gerado do OpenAPI); aqui ficam os tipos
// que o OpenAPI não descreve (respostas sem schema) e utilitários.
export * from './api.generated';

import type { ClassificacaoDto, OrigemLocalizacaoEquipe, StatusOrdemServico, TipoManutencao } from './api.generated';

/** RFC 7807 ProblemDetails, com as extensões usadas pela API do Farol. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  /** 400: campo → mensagens. */
  errors?: Record<string, string[]>;
  /** 422: pendências que impedem a operação (ex.: publicação). */
  pendencias?: string[];
  [extra: string]: unknown;
}

/** Item do histórico de cálculo de prioridade (GET /api/ordens-servico/{id}/prioridade). */
export interface HistoricoCalculoDto {
  id: number;
  calculadoEm: string;
  motivo: string;
  pontuacao: number;
  nivelPrecedencia: number;
  prioridade: string | null;
  prioridadeCodigo: string | null;
  versao: number | null;
  versaoMunicipioId: number | null;
  motivoPrincipal: string | null;
  justificativa: string | null;
}

/** GET /api/ordens-servico/{id}/prioridade — sem schema no OpenAPI; forma observada na API. */
export interface PrioridadeExplicacaoDto {
  atual: ClassificacaoDto | null;
  politicaOrdenacao: string[];
  historico: HistoricoCalculoDto[];
}

/** GET /api/equipes/{id}/localizacao — sem schema no OpenAPI (lista genérica). */
export interface LocalizacaoEquipeHistoricoDto {
  latitude?: number;
  longitude?: number;
  origem?: OrigemLocalizacaoEquipe | string;
  registradaEm?: string;
  [extra: string]: unknown;
}

/** Parâmetros de GET /api/ordens-servico (FiltroFila). Listas viram parâmetros repetidos. */
export interface FiltroFila {
  MunicipioId?: number | null;
  PrioridadeIds?: number[];
  TipoManutencao?: TipoManutencao | '';
  TipoOcorrenciaId?: number | null;
  Status?: StatusOrdemServico[];
  Bairro?: string;
  Logradouro?: string;
  Trecho?: string;
  FaixaPessoas?: string;
  FaixaUcs?: string;
  Risco?: string;
  ServicoEssencial?: string;
  Equipamento?: string;
  EquipeId?: number | null;
  AbertaDe?: string;
  AbertaAte?: string;
  Prazo?: '' | 'vencido' | 'proximo';
  SubestacaoId?: number | null;
  ConjuntoId?: number | null;
  Transformador?: string;
  ClasseId?: number | null;
  SituacaoCliente?: string;
  Uc?: string;
  Busca?: string;
  OrdenarPor?: string;
  IncluirEncerradas?: boolean;
  Pagina?: number;
  TamanhoPagina?: number;
}

/** Ações devolvidas em OrdemServicoDetalheDto.acoesDisponiveis. */
export type AcaoOs =
  | 'DESPACHAR'
  | 'ACEITE'
  | 'INICIAR'
  | 'CONCLUIR'
  | 'REMOVER_EQUIPE'
  | 'RECLASSIFICAR'
  | 'ATUALIZAR_FATOS'
  | 'TROCAR_MUNICIPIO'
  | 'UNIFICAR'
  | 'COMENTAR'
  | `STATUS:${StatusOrdemServico}`;
