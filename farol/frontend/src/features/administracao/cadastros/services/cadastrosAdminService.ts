import { api } from '@/services/api/client';
import type {
  ClasseClienteDto,
  ClasseSalvarRequest,
  ConjuntoDto,
  ConjuntoSalvarRequest,
  LocalidadeDto,
  LocalidadeSalvarRequest,
  MunicipioDto,
  MunicipioSalvarRequest,
  SubestacaoDto,
  SubestacaoSalvarRequest,
  TipoOcorrenciaDto,
  TipoOcorrenciaSalvarRequest,
  TransformadorDtoPaginaDto,
  TransformadorSalvarRequest,
} from '@/types/api';

export interface FiltroTransformadores {
  municipioId: number | null;
  numero: string;
  pagina: number;
  tamanhoPagina: number;
}

/** Salva criando (id null → POST) ou alterando (PUT /{id}). */
function salvar<T>(recurso: string, id: number | null, body: unknown): Promise<T> {
  return id === null ? api.post<T>(`/api/admin/${recurso}`, body) : api.put<T>(`/api/admin/${recurso}/${id}`, body);
}

/** Cadastros da rede (seções 3, 3A.1 e 16): leitura pelas rotas de consulta, escrita em /api/admin. */
export const cadastrosAdminService = {
  municipios: () => api.get<MunicipioDto[]>('/api/municipios', { todos: true }),
  salvarMunicipio: (id: number | null, r: MunicipioSalvarRequest) => salvar<MunicipioDto>('municipios', id, r),
  excluirMunicipio: (id: number) => api.delete<void>(`/api/admin/municipios/${id}`),

  localidades: (municipioId: number | null) =>
    api.get<LocalidadeDto[]>('/api/localidades', { municipioId: municipioId ?? undefined }),
  salvarLocalidade: (id: number | null, r: LocalidadeSalvarRequest) => salvar<number>('localidades', id, r),

  subestacoes: (municipioId: number) => api.get<SubestacaoDto[]>(`/api/municipios/${municipioId}/subestacoes`),
  salvarSubestacao: (id: number | null, r: SubestacaoSalvarRequest) => salvar<number>('subestacoes', id, r),

  conjuntos: (subestacaoId: number) => api.get<ConjuntoDto[]>(`/api/subestacoes/${subestacaoId}/conjuntos`),
  salvarConjunto: (id: number | null, r: ConjuntoSalvarRequest) => salvar<number>('conjuntos', id, r),

  transformadores: (f: FiltroTransformadores) =>
    api.get<TransformadorDtoPaginaDto>('/api/transformadores', {
      municipioId: f.municipioId ?? undefined,
      numero: f.numero.trim() || undefined,
      pagina: f.pagina,
      tamanhoPagina: f.tamanhoPagina,
    }),
  salvarTransformador: (id: number | null, r: TransformadorSalvarRequest) => salvar<number>('transformadores', id, r),

  salvarClasse: (id: number | null, r: ClasseSalvarRequest) => salvar<ClasseClienteDto>('classes-cliente', id, r),

  tiposOcorrencia: () => api.get<TipoOcorrenciaDto[]>('/api/tipos-ocorrencia', { todos: true }),
  salvarTipoOcorrencia: (id: number | null, r: TipoOcorrenciaSalvarRequest) => salvar<number>('tipos-ocorrencia', id, r),
};
