import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import { osService } from '@/features/ordens-servico/services/osService';
import { notificarSucesso } from '@/components/feedback/toast';

/**
 * Detalhe completo da OS. `keepPreviousData` mantém o conteúdo visível enquanto a fila
 * (SignalR / 60 s) invalida o prefixo `operacao` — o painel não fecha nem "pisca".
 */
export function useOsDetalhe(osId: string | null) {
  return useQuery({
    queryKey: queryKeys.os(osId ?? '-'),
    queryFn: () => osService.detalhe(osId!),
    enabled: !!osId,
    placeholderData: keepPreviousData,
  });
}

/** Explicação da posição: política de ordenação, classificação atual e histórico de cálculos. */
export function useOsPrioridade(osId: string | null) {
  return useQuery({
    queryKey: queryKeys.osPrioridade(osId ?? '-'),
    queryFn: () => osService.prioridade(osId!),
    enabled: !!osId,
    placeholderData: keepPreviousData,
  });
}

/** Busca de OS por termo (número), para a unificação. */
export function useOsBusca(busca: string, municipioId: number | null) {
  const termo = busca.trim();
  return useQuery({
    queryKey: ['operacao', 'os-busca', termo, municipioId],
    queryFn: () => osService.buscar(termo, municipioId),
    enabled: termo.length >= 3,
    staleTime: 15_000,
  });
}

/** Invalida tudo que é operacional (detalhe, fila, indicadores, mapa, candidatas). */
export function useInvalidarOperacao() {
  const qc = useQueryClient();
  return () => qc.invalidateQueries({ queryKey: queryKeys.operacao });
}

/**
 * Mutação genérica sobre a OS: ao concluir, mostra o toast e invalida o prefixo operacional.
 * Erros NÃO viram toast aqui — os diálogos exibem o ProblemDetails no próprio formulário.
 */
export function useOsMutacao<TVars, TResult = unknown>(fn: (vars: TVars) => Promise<TResult>, mensagemSucesso: string | ((r: TResult, v: TVars) => string)) {
  const invalidar = useInvalidarOperacao();
  return useMutation({
    mutationFn: fn,
    onSuccess: async (r, v) => {
      notificarSucesso(typeof mensagemSucesso === 'function' ? mensagemSucesso(r, v) : mensagemSucesso);
      await invalidar();
    },
  });
}
