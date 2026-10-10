import { api } from '@/services/api/client';
import type { ConjuntoDto, InterpretacaoTransformadorDto, SubestacaoDto } from '@/types/api';

export const redeService = {
  subestacoes: (municipioId: number) => api.get<SubestacaoDto[]>(`/api/municipios/${municipioId}/subestacoes`),
  conjuntos: (subestacaoId: number) => api.get<ConjuntoDto[]>(`/api/subestacoes/${subestacaoId}/conjuntos`),
  interpretarTransformador: (numero: string, municipioId: number | null, signal?: AbortSignal) =>
    api.get<InterpretacaoTransformadorDto>('/api/transformadores/interpretar', { numero, municipioId }, signal),
};
