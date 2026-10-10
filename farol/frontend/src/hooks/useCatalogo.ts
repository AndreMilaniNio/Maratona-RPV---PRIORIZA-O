import { useQuery } from '@tanstack/react-query';
import { api } from '@/services/api/client';
import { queryKeys } from '@/services/api/queryKeys';
import type { CatalogoDto, CriterioFormularioDto } from '@/types/api';

export const catalogoService = {
  obter: () => api.get<CatalogoDto>('/api/catalogo'),
};

/** Catálogo (municípios, tipos de ocorrência, classes, critérios e opções, prioridades, configuração). */
export function useCatalogo() {
  return useQuery({
    queryKey: queryKeys.catalogo,
    queryFn: catalogoService.obter,
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
