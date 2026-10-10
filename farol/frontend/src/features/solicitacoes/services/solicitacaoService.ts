import { api } from '@/services/api/client';
import type { ClassificacaoDto, DuplicidadeDto, NovaSolicitacaoRequest, NovaSolicitacaoResponse } from '@/types/api';

export interface ParametrosDuplicidade {
  municipioId: number;
  latitude?: number | null;
  longitude?: number | null;
  transformador?: string | null;
  uc?: string[];
  conjuntoId?: number | null;
  tipoOcorrenciaId?: number | null;
}

export const solicitacaoService = {
  /** Registra a solicitação e gera (ou vincula) a OS. A mesma chaveIdempotencia devolve o mesmo resultado. */
  registrar: (req: NovaSolicitacaoRequest) => api.post<NovaSolicitacaoResponse>('/api/solicitacoes', req),
  /** Prévia da classificação (nada é gravado). */
  previa: (req: NovaSolicitacaoRequest) => api.post<ClassificacaoDto>('/api/solicitacoes/previa', req),
  possiveisDuplicidades: (p: ParametrosDuplicidade, signal?: AbortSignal) =>
    api.get<DuplicidadeDto[]>(
      '/api/ordens-servico/possiveis-duplicidades',
      {
        municipioId: p.municipioId,
        latitude: p.latitude,
        longitude: p.longitude,
        transformador: p.transformador,
        uc: p.uc,
        conjuntoId: p.conjuntoId,
        tipoOcorrenciaId: p.tipoOcorrenciaId,
      },
      signal,
    ),
};
