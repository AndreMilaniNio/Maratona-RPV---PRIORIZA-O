import * as React from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@/features/autenticacao/hooks/AuthProvider';
import { authService } from '@/features/autenticacao/services/authService';
import { notificarErro } from '@/components/feedback/toast';
import type { MunicipioResumoDto } from '@/types/api';

export interface CidadeContextValue {
  /** Município selecionado no cabeçalho; `null` = "Todas as cidades" (só com cidades.todas). */
  municipioId: number | null;
  municipio: MunicipioResumoDto | null;
  opcoes: MunicipioResumoDto[];
  podeTodas: boolean;
  selecionar: (id: number | null) => void;
  salvando: boolean;
}

const CidadeContext = React.createContext<CidadeContextValue | null>(null);

function escolhaInicial(opcoes: MunicipioResumoDto[], podeTodas: boolean, preferido: number | null | undefined): number | null {
  if (preferido != null && opcoes.some((m) => m.id === preferido)) return preferido;
  if (podeTodas) return null;
  return opcoes[0]?.id ?? null;
}

/**
 * Seletor de cidade do cabeçalho. A escolha é persistida no perfil do usuário
 * (PUT /api/me/preferencias), não em localStorage.
 */
export function CidadeProvider({ children }: { children: React.ReactNode }) {
  const { usuario, atualizarUsuario } = useAuth();
  const queryClient = useQueryClient();
  const opcoes = React.useMemo(() => usuario?.municipios ?? [], [usuario?.municipios]);
  const podeTodas = !!usuario?.todasCidades;
  const [municipioId, setMunicipioId] = React.useState<number | null>(() =>
    escolhaInicial(opcoes, podeTodas, usuario?.municipioPreferidoId),
  );
  const [salvando, setSalvando] = React.useState(false);

  // Usuário trocou (login de outra pessoa): recalcula a escolha.
  const usuarioId = usuario?.id;
  React.useEffect(() => {
    setMunicipioId(escolhaInicial(opcoes, podeTodas, usuario?.municipioPreferidoId));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [usuarioId]);

  const selecionar = React.useCallback(
    (id: number | null) => {
      if (id === null && !podeTodas) return;
      setMunicipioId(id);
      setSalvando(true);
      authService
        .salvarPreferencias(id)
        .then(() => atualizarUsuario({ municipioPreferidoId: id }))
        .catch((e) => notificarErro(e, 'Não foi possível salvar a cidade preferida'))
        .finally(() => setSalvando(false));
      void queryClient.invalidateQueries({ queryKey: ['operacao'] });
    },
    [podeTodas, atualizarUsuario, queryClient],
  );

  const value = React.useMemo<CidadeContextValue>(
    () => ({
      municipioId,
      municipio: opcoes.find((m) => m.id === municipioId) ?? null,
      opcoes,
      podeTodas,
      selecionar,
      salvando,
    }),
    [municipioId, opcoes, podeTodas, selecionar, salvando],
  );
  return <CidadeContext.Provider value={value}>{children}</CidadeContext.Provider>;
}

export function useCidade(): CidadeContextValue {
  const ctx = React.useContext(CidadeContext);
  if (!ctx) throw new Error('useCidade precisa estar dentro de <CidadeProvider>.');
  return ctx;
}
