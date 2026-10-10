import { api } from '@/services/api/client';
import type { ConjuntoDto, EquipeDto, FilaDto, FiltroFila, IndicadoresDto, SubestacaoDto } from '@/types/api';

/** Fila operacional, indicadores e listas auxiliares dos filtros do painel. */
export const filaService = {
  /** GET /api/ordens-servico — filtros, ordenação e paginação no servidor; listas como parâmetros repetidos. */
  listar: (filtro: FiltroFila, signal?: AbortSignal) => api.get<FilaDto>('/api/ordens-servico', { ...filtro }, signal),

  /** GET /api/ordens-servico/indicadores — consolidados e por cidade (municipioId omitido = todas). */
  indicadores: (municipioId: number | null) => api.get<IndicadoresDto>('/api/ordens-servico/indicadores', { municipioId }),

  /** Equipes no escopo da cidade (filtro "equipe atribuída"). */
  equipes: (municipioId: number | null) => api.get<EquipeDto[]>('/api/equipes', { municipioId }),

  /** Circuitos (subestações) da cidade. */
  subestacoes: (municipioId: number) => api.get<SubestacaoDto[]>(`/api/municipios/${municipioId}/subestacoes`),

  /** Conjuntos elétricos de um circuito. */
  conjuntos: (subestacaoId: number) => api.get<ConjuntoDto[]>(`/api/subestacoes/${subestacaoId}/conjuntos`),
};
