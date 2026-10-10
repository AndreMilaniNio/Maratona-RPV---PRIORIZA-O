import { useQuery } from '@tanstack/react-query';
import { ApiError } from '@/services/api/client';
import { queryKeys } from '@/services/api/queryKeys';
import type { UnidadeConsumidoraDto } from '@/types/api';
import { ucService } from '../services/ucService';

export type ResultadoUc =
  | { estado: 'consultando' }
  | { estado: 'encontrada'; uc: UnidadeConsumidoraDto }
  | { estado: 'nao-validada' }
  | { estado: 'formato-invalido'; mensagem: string }
  | { estado: 'erro'; erro: unknown }
  | { estado: 'nao-consultada' };

/**
 * Consulta uma UC (cada consulta é auditada: o resultado fica em cache e não é refeito a cada render).
 * `habilitada` = false (ex.: simulador) não consulta.
 */
export function useUnidadeConsumidora(numero: string, habilitada = true): ResultadoUc & { refetch: () => void } {
  const n = numero.trim();
  const q = useQuery({
    queryKey: queryKeys.uc(n),
    queryFn: () => ucService.consultar(n),
    enabled: habilitada && n !== '',
    retry: false,
    staleTime: Infinity,
    gcTime: 30 * 60_000,
    refetchOnWindowFocus: false,
  });
  const refetch = () => void q.refetch();
  if (!habilitada || n === '') return { estado: 'nao-consultada', refetch };
  if (q.isPending) return { estado: 'consultando', refetch };
  if (q.isError) {
    const e = q.error;
    if (e instanceof ApiError && e.status === 404) return { estado: 'nao-validada', refetch };
    if (e instanceof ApiError && e.status === 400)
      return { estado: 'formato-invalido', mensagem: 'Formato de UC inválido', refetch };
    return { estado: 'erro', erro: e, refetch };
  }
  return { estado: 'encontrada', uc: q.data, refetch };
}
