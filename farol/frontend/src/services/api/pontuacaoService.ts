import { api } from '@/services/api/client';
import type { ConfiguracaoPontuacaoDto, PublicarRequest, SalvarRascunhoRequest, VersaoDetalheDto, VersaoListaDto } from '@/types/api';

/** Chamadas da configuracao de prioridade; o servidor segue como fonte da verdade. */
export const pontuacaoService = {
  obter: (municipioId: number | null) => api.get<ConfiguracaoPontuacaoDto>('/api/configuracoes/pontuacao', { municipioId }),
  criarRascunho: (municipioId: number | null) => api.post<VersaoDetalheDto>('/api/configuracoes/pontuacao/rascunho', undefined, { municipioId }),
  salvarRascunho: (request: SalvarRascunhoRequest) => api.put<VersaoDetalheDto>('/api/configuracoes/pontuacao/rascunho', request),
  publicar: (request: PublicarRequest) => api.post<VersaoDetalheDto>('/api/configuracoes/pontuacao/publicar', request),
  versoes: (municipioId: number | null) => api.get<VersaoListaDto[]>('/api/configuracoes/pontuacao/versoes', { municipioId }),
};
