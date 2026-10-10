import { api } from '@/services/api/client';
import type { CandidatasDto, DesignacaoDto, DesignarRequest } from '@/types/api';

/** Chamadas HTTP do despacho ("Disponibilizar para equipe") e do ciclo de vida do despacho. */
export const despachoService = {
  candidatas: (osId: string, incluirApoio: boolean) =>
    api.get<CandidatasDto>(`/api/ordens-servico/${osId}/equipes-candidatas`, { incluirApoio }),
  designar: (osId: string, body: DesignarRequest) => api.post<DesignacaoDto>(`/api/ordens-servico/${osId}/despachos`, body),

  aceite: (despachoId: string, observacao?: string | null) => api.post<void>(`/api/despachos/${despachoId}/aceite`, { observacao: observacao || null }),
  inicio: (despachoId: string, observacao?: string | null) => api.post<void>(`/api/despachos/${despachoId}/inicio`, { observacao: observacao || null }),
  conclusao: (despachoId: string, observacao?: string | null) =>
    api.post<void>(`/api/despachos/${despachoId}/conclusao`, { observacao: observacao || null }),
  /** Remove a equipe da OS (troca/desistência); o histórico é preservado e a OS volta a "Aguardando despacho". */
  encerrar: (despachoId: string, motivo: string) => api.post<void>(`/api/despachos/${despachoId}/encerrar`, { motivo }),
};
