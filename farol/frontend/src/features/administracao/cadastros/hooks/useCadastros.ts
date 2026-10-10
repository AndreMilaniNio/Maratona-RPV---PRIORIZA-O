import { keepPreviousData, useMutation, useQuery, useQueryClient, type QueryKey } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import { cadastrosAdminService, type FiltroTransformadores } from '../services/cadastrosAdminService';
import type {
  ClasseSalvarRequest,
  ConjuntoSalvarRequest,
  LocalidadeSalvarRequest,
  MunicipioSalvarRequest,
  SubestacaoSalvarRequest,
  TipoOcorrenciaSalvarRequest,
  TransformadorSalvarRequest,
} from '@/types/api';

// ---------- Consultas ----------
export function useMunicipiosAdmin() {
  return useQuery({ queryKey: queryKeys.municipios(true), queryFn: cadastrosAdminService.municipios });
}

export function useLocalidadesAdmin(municipioId: number | null) {
  return useQuery({ queryKey: queryKeys.localidades(municipioId), queryFn: () => cadastrosAdminService.localidades(municipioId) });
}

export function useSubestacoesAdmin(municipioId: number | null) {
  return useQuery({
    queryKey: queryKeys.subestacoes(municipioId ?? 0),
    queryFn: () => cadastrosAdminService.subestacoes(municipioId!),
    enabled: municipioId !== null,
  });
}

export function useConjuntosAdmin(subestacaoId: number | null) {
  return useQuery({
    queryKey: queryKeys.conjuntos(subestacaoId ?? 0),
    queryFn: () => cadastrosAdminService.conjuntos(subestacaoId!),
    enabled: subestacaoId !== null,
  });
}

export function useTransformadoresAdmin(filtro: FiltroTransformadores) {
  return useQuery({
    queryKey: queryKeys.transformadores(filtro),
    queryFn: () => cadastrosAdminService.transformadores(filtro),
    placeholderData: keepPreviousData,
  });
}

export function useTiposOcorrenciaAdmin() {
  return useQuery({ queryKey: queryKeys.tiposOcorrencia(true), queryFn: cadastrosAdminService.tiposOcorrencia });
}

// ---------- Gravações ----------
interface Salvar<R> {
  id: number | null;
  dados: R;
}

const REDE: QueryKey = ['rede'];
const PONTUACAO: QueryKey = ['pontuacao'];
const MAPA: QueryKey = ['operacao', 'mapa'];

/** Mutação que, ao concluir, invalida as chaves informadas. */
function useSalvarComInvalidacao<R, T>(fn: (id: number | null, r: R) => Promise<T>, chaves: QueryKey[]) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dados }: Salvar<R>) => fn(id, dados),
    onSuccess: () => Promise.all(chaves.map((queryKey) => qc.invalidateQueries({ queryKey }))),
  });
}

export const useSalvarMunicipio = () =>
  useSalvarComInvalidacao<MunicipioSalvarRequest, unknown>(cadastrosAdminService.salvarMunicipio, [['municipios'], queryKeys.catalogo, MAPA]);

export function useExcluirMunicipio() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => cadastrosAdminService.excluirMunicipio(id),
    onSuccess: () => Promise.all([qc.invalidateQueries({ queryKey: ['municipios'] }), qc.invalidateQueries({ queryKey: queryKeys.catalogo })]),
  });
}

export const useSalvarLocalidade = () =>
  useSalvarComInvalidacao<LocalidadeSalvarRequest, number>(cadastrosAdminService.salvarLocalidade, [REDE]);

export const useSalvarSubestacao = () =>
  useSalvarComInvalidacao<SubestacaoSalvarRequest, number>(cadastrosAdminService.salvarSubestacao, [REDE, MAPA]);

export const useSalvarConjunto = () =>
  useSalvarComInvalidacao<ConjuntoSalvarRequest, number>(cadastrosAdminService.salvarConjunto, [REDE]);

export const useSalvarTransformador = () =>
  useSalvarComInvalidacao<TransformadorSalvarRequest, number>(cadastrosAdminService.salvarTransformador, [REDE]);

/** Classes viram opções do critério CLASSE_CLIENTE: catálogo, critérios e pontuação mudam. */
export const useSalvarClasse = () =>
  useSalvarComInvalidacao<ClasseSalvarRequest, unknown>(cadastrosAdminService.salvarClasse, [
    queryKeys.catalogo,
    queryKeys.criterios,
    PONTUACAO,
  ]);

/** Tipos viram opções do critério TIPO_OCORRENCIA. */
export const useSalvarTipoOcorrencia = () =>
  useSalvarComInvalidacao<TipoOcorrenciaSalvarRequest, number>(cadastrosAdminService.salvarTipoOcorrencia, [
    ['config', 'tipos-ocorrencia'],
    queryKeys.catalogo,
    queryKeys.criterios,
    PONTUACAO,
  ]);
