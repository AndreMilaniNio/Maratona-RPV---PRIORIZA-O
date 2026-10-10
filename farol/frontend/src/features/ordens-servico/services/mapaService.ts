import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { api } from '@/services/api/client';
import { queryKeys } from '@/services/api/queryKeys';
import { INTERVALO_ATUALIZACAO_MS } from '@/app/config/env';
import type { MapaDto, RotaDto } from '@/types/api';

export const mapaService = {
  operacoes: (municipioId: number | null) => api.get<MapaDto>('/api/mapa/operacoes', { municipioId }),
  rota: (de: [number, number], para: [number, number]) =>
    api.get<RotaDto>('/api/mapa/rota', { deLat: de[0], deLng: de[1], paraLat: para[0], paraLng: para[1] }),
};

/** OS, equipes e subestações do mapa, no recorte da cidade; atualiza a cada 60 s. */
export function useMapaOperacoes(municipioId: number | null, enabled = true) {
  return useQuery({
    queryKey: queryKeys.mapa(municipioId),
    queryFn: () => mapaService.operacoes(municipioId),
    refetchInterval: INTERVALO_ATUALIZACAO_MS,
    placeholderData: keepPreviousData,
    enabled,
  });
}

/** Rota/linha de comparação entre dois pontos (sem serviço de roteamento: linha reta, sem tempo). */
export function useRota(de: [number, number] | null, para: [number, number] | null) {
  return useQuery({
    queryKey: ['rota', de, para],
    queryFn: () => mapaService.rota(de!, para!),
    enabled: !!de && !!para,
    staleTime: 5 * 60_000,
  });
}
