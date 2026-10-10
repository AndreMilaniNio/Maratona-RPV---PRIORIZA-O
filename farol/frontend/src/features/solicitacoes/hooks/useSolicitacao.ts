import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import type { NovaSolicitacaoRequest } from '@/types/api';
import { solicitacaoService, type ParametrosDuplicidade } from '../services/solicitacaoService';
import { useDebouncedValue } from './useDebouncedValue';

/**
 * Prévia da classificação, com atraso (~700 ms) após a última alteração.
 * `req` nulo = dados insuficientes (município, tipo de ocorrência e tipo de manutenção).
 */
export function usePreviaClassificacao(req: NovaSolicitacaoRequest | null) {
  const json = req ? JSON.stringify(req) : '';
  const atrasado = useDebouncedValue(json, 700);
  const query = useQuery({
    queryKey: ['solicitacao', 'previa', atrasado],
    queryFn: () => solicitacaoService.previa(JSON.parse(atrasado) as NovaSolicitacaoRequest),
    enabled: atrasado !== '',
    placeholderData: keepPreviousData,
    retry: false,
    staleTime: 30_000,
  });
  return { ...query, aguardando: json !== atrasado, habilitada: json !== '' };
}

/** Possíveis duplicidades (consulta atrasada; só com município e algum dado de localização/rede). */
export function usePossiveisDuplicidades(params: ParametrosDuplicidade | null) {
  const json = params ? JSON.stringify(params) : '';
  const atrasado = useDebouncedValue(json, 800);
  const p = atrasado ? (JSON.parse(atrasado) as ParametrosDuplicidade) : null;
  return useQuery({
    queryKey: queryKeys.duplicidades(p),
    queryFn: ({ signal }) => solicitacaoService.possiveisDuplicidades(p as ParametrosDuplicidade, signal),
    enabled: p !== null,
    placeholderData: keepPreviousData,
    retry: false,
    staleTime: 30_000,
  });
}

/** Registro da solicitação; invalida a fila operacional ao concluir. */
export function useRegistrarSolicitacao() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: solicitacaoService.registrar,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: queryKeys.operacao });
      void qc.invalidateQueries({ queryKey: ['duplicidades'] });
    },
  });
}
