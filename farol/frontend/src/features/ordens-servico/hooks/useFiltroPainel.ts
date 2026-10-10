import * as React from 'react';
import { useSearchParams } from 'react-router';
import {
  FILTRO_PADRAO,
  filtroDeSearchParams,
  filtroParaSearchParams,
  type FiltroPainel,
} from '@/features/ordens-servico/schemas/filtroFila';

/**
 * Filtros do painel guardados na URL: recarregar, voltar e a atualização automática
 * nunca perdem filtros, página nem o detalhe aberto.
 */
export function useFiltroPainel() {
  const [searchParams, setSearchParams] = useSearchParams();
  const chave = searchParams.toString();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const filtro = React.useMemo(() => filtroDeSearchParams(searchParams), [chave]);

  /** Aplica alterações; qualquer mudança que não seja de página volta à página 1. */
  const alterar = React.useCallback(
    (patch: Partial<FiltroPainel>) => {
      setSearchParams(
        (atual) => {
          const anterior = filtroDeSearchParams(atual);
          const mudaPagina = Object.keys(patch).length === 1 && 'pagina' in patch;
          const novo: FiltroPainel = { ...anterior, ...patch, pagina: mudaPagina ? (patch.pagina ?? 1) : 1 };
          return filtroParaSearchParams(novo, atual);
        },
        { replace: true },
      );
    },
    [setSearchParams],
  );

  /** "Limpar filtros": volta à fila operacional padrão (mantém tamanho de página). */
  const limpar = React.useCallback(() => {
    setSearchParams(
      (atual) => filtroParaSearchParams({ ...FILTRO_PADRAO, tamanhoPagina: filtroDeSearchParams(atual).tamanhoPagina }, atual),
      { replace: true },
    );
  }, [setSearchParams]);

  return { filtro, alterar, limpar };
}

/** OS aberta no painel lateral (`?os=<id>`), preservada entre atualizações. */
export function useOsAbertaNaUrl() {
  const [searchParams, setSearchParams] = useSearchParams();
  const osId = searchParams.get('os');
  const abrir = React.useCallback(
    (id: string | null) => {
      setSearchParams(
        (atual) => {
          const sp = new URLSearchParams(atual);
          if (id) sp.set('os', id);
          else sp.delete('os');
          return sp;
        },
        { replace: true },
      );
    },
    [setSearchParams],
  );
  return { osId, abrir };
}
