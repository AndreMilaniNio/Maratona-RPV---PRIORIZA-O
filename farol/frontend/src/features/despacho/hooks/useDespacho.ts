import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import { despachoService } from '@/features/despacho/services/despachoService';
import type { DesignarRequest } from '@/types/api';

/** Equipes candidatas (compatibilidade e segurança antes da distância). */
export function useCandidatas(osId: string | null, incluirApoio: boolean) {
  return useQuery({
    queryKey: queryKeys.candidatas(osId ?? '-', incluirApoio),
    queryFn: () => despachoService.candidatas(osId!, incluirApoio),
    enabled: !!osId,
    placeholderData: keepPreviousData,
  });
}

/** Confirma a designação. O chamador trata 409/400/403 e o sucesso (tela de entrega). */
export function useDesignar(osId: string | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: DesignarRequest) => despachoService.designar(osId!, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.operacao }),
  });
}
