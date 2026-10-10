import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import { INTERVALO_ATUALIZACAO_MS } from '@/app/config/env';
import { filaService } from '@/features/ordens-servico/services/filaService';
import type { FiltroFila } from '@/types/api';

/**
 * Fila operacional (seção 5.2). Atualiza a cada 60 s; `keepPreviousData` mantém a
 * tabela visível ao trocar filtros/página e durante falhas de atualização.
 */
export function useFila(filtro: FiltroFila, enabled = true) {
  return useQuery({
    queryKey: queryKeys.fila(filtro),
    queryFn: ({ signal }) => filaService.listar(filtro, signal),
    refetchInterval: INTERVALO_ATUALIZACAO_MS,
    placeholderData: keepPreviousData,
    enabled,
  });
}

/** Indicadores do painel (seção 5.1), no recorte da cidade do cabeçalho. */
export function useIndicadores(municipioId: number | null) {
  return useQuery({
    queryKey: queryKeys.indicadores(municipioId),
    queryFn: () => filaService.indicadores(municipioId),
    refetchInterval: INTERVALO_ATUALIZACAO_MS,
    placeholderData: keepPreviousData,
  });
}

/** Equipes da cidade, para o filtro "equipe atribuída". */
export function useEquipesFiltro(municipioId: number | null) {
  return useQuery({
    queryKey: queryKeys.equipes(municipioId),
    queryFn: () => filaService.equipes(municipioId),
    staleTime: 60_000,
  });
}

/** Circuitos da cidade (só com uma cidade selecionada). */
export function useSubestacoesFiltro(municipioId: number | null) {
  return useQuery({
    queryKey: queryKeys.subestacoes(municipioId ?? 0),
    queryFn: () => filaService.subestacoes(municipioId!),
    enabled: municipioId != null,
    staleTime: 5 * 60_000,
  });
}

/** Conjuntos de um circuito. */
export function useConjuntosFiltro(subestacaoId: number | null) {
  return useQuery({
    queryKey: queryKeys.conjuntos(subestacaoId ?? 0),
    queryFn: () => filaService.conjuntos(subestacaoId!),
    enabled: subestacaoId != null,
    staleTime: 5 * 60_000,
  });
}
