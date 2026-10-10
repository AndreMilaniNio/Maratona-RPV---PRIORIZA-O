import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import { equipesService } from '@/features/equipes/services/equipesService';
import type { EquipeSalvarRequest, ItemSalvarRequest, LocalizacaoEquipeRequest, StatusEquipeRequest } from '@/types/api';

/** Equipes visíveis no recorte da cidade (sob o prefixo `operacao`: o SignalR atualiza). */
export function useEquipes(municipioId: number | null, enabled = true) {
  return useQuery({
    queryKey: queryKeys.equipes(municipioId),
    queryFn: () => equipesService.listar(municipioId),
    placeholderData: keepPreviousData,
    enabled,
  });
}

export function useEquipe(id: number | null) {
  return useQuery({
    queryKey: queryKeys.equipe(id ?? 0),
    queryFn: () => equipesService.obter(id!),
    enabled: id != null && id > 0,
  });
}

/** Histórico de posições (exige `localizacao.consultar`). */
export function useLocalizacoesEquipe(id: number | null, enabled = true) {
  return useQuery({
    queryKey: queryKeys.equipeLocalizacoes(id ?? 0),
    queryFn: () => equipesService.localizacoes(id!),
    enabled: enabled && id != null && id > 0,
  });
}

function useInvalidarOperacao() {
  const qc = useQueryClient();
  return () => qc.invalidateQueries({ queryKey: queryKeys.operacao });
}

export function useAlterarStatusEquipe(id: number) {
  const invalidar = useInvalidarOperacao();
  return useMutation({
    mutationFn: (req: StatusEquipeRequest) => equipesService.alterarStatus(id, req),
    onSuccess: () => invalidar(),
  });
}

export function useAtualizarLocalizacaoEquipe(id: number) {
  const invalidar = useInvalidarOperacao();
  return useMutation({
    mutationFn: (req: LocalizacaoEquipeRequest) => equipesService.atualizarLocalizacao(id, req),
    onSuccess: () => invalidar(),
  });
}

/** Cria (id = null) ou altera uma equipe. */
export function useSalvarEquipe(id: number | null) {
  const invalidar = useInvalidarOperacao();
  return useMutation({
    mutationFn: (req: EquipeSalvarRequest) => (id == null ? equipesService.criar(req) : equipesService.alterar(id, req)),
    onSuccess: () => invalidar(),
  });
}

export type TipoItemEquipe = 'qualificacao' | 'recurso';

/** Cria qualificação ou recurso no catálogo e recarrega o catálogo. */
export function useCriarItemEquipe(tipo: TipoItemEquipe) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: ItemSalvarRequest) =>
      tipo === 'qualificacao' ? equipesService.criarQualificacao(req) : equipesService.criarRecurso(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.catalogo }),
  });
}
