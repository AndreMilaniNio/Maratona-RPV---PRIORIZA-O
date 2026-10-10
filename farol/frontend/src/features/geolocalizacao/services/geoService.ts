import { api } from '@/services/api/client';
import type { CepDto, CoordenadasInterpretadasDto, InterpretarCoordenadasRequest } from '@/types/api';

export const geoService = {
  /** Consulta de CEP (8 dígitos, serviço configurável no backend). */
  cep: (cep: string, signal?: AbortSignal) => api.get<CepDto>(`/api/geocodificacao/cep/${cep}`, undefined, signal),
  /** Interpreta texto colado (decimal, link de mapa, GMS) em coordenadas WGS84. */
  interpretarCoordenadas: (req: InterpretarCoordenadasRequest) =>
    api.post<CoordenadasInterpretadasDto>('/api/geocodificacao/coordenadas/interpretar', req),
};
