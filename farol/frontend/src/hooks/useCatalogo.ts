import { useQuery } from '@tanstack/react-query';
import { api } from '@/services/api/client';
import { queryKeys } from '@/services/api/queryKeys';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import type { CatalogoDto, CriterioFormularioDto } from '@/types/api';

export const catalogoService = {
  obter: (municipioId: number | null) => api.get<CatalogoDto>('/api/catalogo', municipioId === null ? undefined : { municipioId }),
};

/** Catálogo (municípios, tipos de ocorrência, classes, critérios e opções, prioridades, configuração). */
export function useCatalogo() {
  const { municipioId } = useCidade();
  return useQuery({
    queryKey: [...queryKeys.catalogo, municipioId],
    queryFn: () => catalogoService.obter(municipioId),
    staleTime: 5 * 60_000,
  });
}

/** Critério fixo do catálogo pelo código (ex.: PESSOAS_AFETADAS). */
export function criterioPorCodigo(catalogo: CatalogoDto | undefined, codigo: string): CriterioFormularioDto | undefined {
  return catalogo?.criteriosFixos.find((c) => c.codigo === codigo);
}

/** Código da opção "desconhecida" de um critério (padrão do formulário). */
export function opcaoDesconhecida(criterio: CriterioFormularioDto | undefined): string | null {
  return criterio?.opcoes.find((o) => o.representaDesconhecido)?.codigo ?? null;
}

/** Rótulo de uma opção de critério a partir do código. */
export function rotuloOpcao(catalogo: CatalogoDto | undefined, criterio: string, codigo: string | null | undefined): string {
  if (!codigo) return '—';
  const c = criterioPorCodigo(catalogo, criterio);
  return c?.opcoes.find((o) => o.codigo === codigo)?.rotulo ?? codigo;
}
