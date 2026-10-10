import { api } from '@/services/api/client';
import type {
  AlterarStatusRequest,
  AnexoDto,
  AtualizarFatosRequest,
  ComentarioDto,
  FilaDto,
  HistoricoDto,
  OrdemServicoDetalheDto,
  PrioridadeExplicacaoDto,
  ReclassificarRequest,
  TrocarMunicipioRequest,
  UnificarRequest,
} from '@/types/api';

const base = (id: string) => `/api/ordens-servico/${id}`;

/** Chamadas HTTP do detalhe da OS e das ações sobre ela. */
export const osService = {
  detalhe: (id: string) => api.get<OrdemServicoDetalheDto>(base(id)),
  prioridade: (id: string) => api.get<PrioridadeExplicacaoDto>(`${base(id)}/prioridade`),
  historico: (id: string) => api.get<HistoricoDto[]>(`${base(id)}/historico`),

  /** Busca de OS por número/termo (usada na unificação). Inclui encerradas = false. */
  buscar: (busca: string, municipioId: number | null) =>
    api.get<FilaDto>('/api/ordens-servico', { Busca: busca, MunicipioId: municipioId, TamanhoPagina: 10, Pagina: 1 }),

  comentar: (id: string, texto: string) => api.post<ComentarioDto>(`${base(id)}/comentarios`, { texto }),
  anexar: (id: string, arquivo: File) => {
    const form = new FormData();
    // nome do campo conforme o controller: IFormFile arquivo
    form.append('arquivo', arquivo);
    return api.upload<AnexoDto>(`${base(id)}/anexos`, form);
  },
  baixarAnexo: (id: string, anexo: Pick<AnexoDto, 'id' | 'nomeArquivo'>) =>
    api.download(`${base(id)}/anexos/${anexo.id}`, anexo.nomeArquivo ?? 'anexo'),

  alterarStatus: (id: string, body: AlterarStatusRequest) => api.patch<void>(`${base(id)}/status`, body),
  reclassificar: (id: string, body: ReclassificarRequest) => api.post<void>(`${base(id)}/reclassificar`, body),
  atualizarFatos: (id: string, body: AtualizarFatosRequest) => api.patch<void>(`${base(id)}/fatos`, body),
  trocarMunicipio: (id: string, body: TrocarMunicipioRequest) => api.patch<void>(`${base(id)}/municipio`, body),
  unificar: (id: string, body: UnificarRequest) => api.post<void>(`${base(id)}/unificar`, body),
};
