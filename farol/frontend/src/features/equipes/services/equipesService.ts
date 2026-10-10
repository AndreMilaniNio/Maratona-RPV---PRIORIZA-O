import { api } from '@/services/api/client';
import type {
  EquipeDto,
  EquipeSalvarRequest,
  ItemCodigoDto,
  ItemSalvarRequest,
  LocalizacaoEquipeHistoricoDto,
  LocalizacaoEquipeRequest,
  StatusEquipeRequest,
} from '@/types/api';

/** Chamadas HTTP de equipes (consulta, status, localização e cadastro administrativo). */
export const equipesService = {
  listar: (municipioId: number | null) => api.get<EquipeDto[]>('/api/equipes', { municipioId }),
  disponiveis: (municipioId: number | null) => api.get<EquipeDto[]>('/api/equipes/disponiveis', { municipioId }),
  obter: (id: number) => api.get<EquipeDto>(`/api/equipes/${id}`),
  /** Últimas 50 posições registradas (mais recentes primeiro). */
  localizacoes: (id: number) => api.get<LocalizacaoEquipeHistoricoDto[]>(`/api/equipes/${id}/localizacao`),
  atualizarLocalizacao: (id: number, req: LocalizacaoEquipeRequest) => api.put<void>(`/api/equipes/${id}/localizacao`, req),
  alterarStatus: (id: number, req: StatusEquipeRequest) => api.patch<void>(`/api/equipes/${id}/status`, req),

  criar: (req: EquipeSalvarRequest) => api.post<EquipeDto>('/api/admin/equipes', req),
  alterar: (id: number, req: EquipeSalvarRequest) => api.put<EquipeDto>(`/api/admin/equipes/${id}`, req),
  criarQualificacao: (req: ItemSalvarRequest) => api.post<ItemCodigoDto>('/api/admin/qualificacoes', req),
  criarRecurso: (req: ItemSalvarRequest) => api.post<ItemCodigoDto>('/api/admin/recursos', req),
};
