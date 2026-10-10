import { useQuery } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import { useDebouncedValue } from '@/features/solicitacoes/hooks/useDebouncedValue';
import { redeService } from '../services/redeService';

/** Circuitos (subestações) do município. */
export function useSubestacoes(municipioId: number | null) {
  return useQuery({
    queryKey: queryKeys.subestacoes(municipioId ?? 0),
    queryFn: () => redeService.subestacoes(municipioId as number),
    enabled: municipioId !== null,
    staleTime: 10 * 60_000,
  });
}

/** Conjuntos elétricos de um circuito. */
export function useConjuntos(subestacaoId: number | null) {
  return useQuery({
    queryKey: queryKeys.conjuntos(subestacaoId ?? 0),
    queryFn: () => redeService.conjuntos(subestacaoId as number),
    enabled: subestacaoId !== null,
    staleTime: 10 * 60_000,
  });
}

/** Interpretação do número do transformador enquanto o atendente digita (atrasada ~400 ms). */
export function useInterpretacaoTransformador(numero: string, municipioId: number | null) {
  const atrasado = useDebouncedValue(numero.trim(), 400);
  const query = useQuery({
    queryKey: queryKeys.interpretarTransformador(atrasado, municipioId),
    queryFn: ({ signal }) => redeService.interpretarTransformador(atrasado, municipioId, signal),
    enabled: atrasado !== '',
    retry: false,
    staleTime: 5 * 60_000,
  });
  return { ...query, aguardando: numero.trim() !== atrasado };
}
