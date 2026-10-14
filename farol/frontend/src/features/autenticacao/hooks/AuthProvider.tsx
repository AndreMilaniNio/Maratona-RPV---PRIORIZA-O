import * as React from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { setUnauthorizedHandler, tokenStorage } from '@/services/api/client';
import { authService } from '@/features/autenticacao/services/authService';
import { atende, type RequisitoPermissao } from '@/lib/permissions';
import type { UsuarioSessaoDto } from '@/types/api';

type EstadoSessao = 'verificando' | 'autenticado' | 'anonimo';

export interface AuthContextValue {
  estado: EstadoSessao;
  usuario: UsuarioSessaoDto | null;
  login: (email: string, senha: string) => Promise<UsuarioSessaoDto>;
  logout: (motivo?: string) => void;
  /** Mensagem exibida no Login após logout forçado (ex.: sessão expirada). */
  motivoSaida: string | null;
  temPermissao: (p: string) => boolean;
  atendeRequisito: (req?: RequisitoPermissao) => boolean;
  atualizarUsuario: (u: Partial<UsuarioSessaoDto>) => void;
}

export const AuthContext = React.createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient();
  const [usuario, setUsuario] = React.useState<UsuarioSessaoDto | null>(null);
  const [estado, setEstado] = React.useState<EstadoSessao>('verificando');
  const [motivoSaida, setMotivoSaida] = React.useState<string | null>(null);

  const logout = React.useCallback(
    (motivo?: string) => {
      tokenStorage.clear();
      setUsuario(null);
      setEstado('anonimo');
      setMotivoSaida(motivo ?? null);
      queryClient.clear();
    },
    [queryClient],
  );

  React.useEffect(() => {
    setUnauthorizedHandler(() => logout('Sua sessão expirou ou não é mais válida. Entre novamente.'));
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  // Restaura a sessão a partir do token guardado.
  React.useEffect(() => {
    if (estado !== 'verificando') return;
    let ativo = true;
    authService
      .me()
      .then((u) => {
        if (!ativo) return;
        setUsuario(u);
        setEstado('autenticado');
      })
      .catch(() => {
        if (!ativo) return;
        tokenStorage.clear();
        setEstado('anonimo');
      });
    return () => {
      ativo = false;
    };
  }, [estado]);

  const login = React.useCallback(async (email: string, senha: string) => {
    const resp = await authService.login(email, senha);
    if (!resp.token || !resp.usuario) throw new Error('Resposta de login inválida.');
    tokenStorage.set(resp.token);
    setUsuario(resp.usuario);
    setMotivoSaida(null);
    setEstado('autenticado');
    return resp.usuario;
  }, []);

  const value = React.useMemo<AuthContextValue>(() => {
    const perms = usuario?.permissoes ?? [];
    return {
      estado,
      usuario,
      login,
      logout,
      motivoSaida,
      temPermissao: (p: string) => perms.includes(p),
      atendeRequisito: (req?: RequisitoPermissao) => atende(perms, req),
      atualizarUsuario: (u) => setUsuario((prev) => (prev ? { ...prev, ...u } : prev)),
    };
  }, [estado, usuario, login, logout, motivoSaida]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = React.useContext(AuthContext);
  if (!ctx) throw new Error('useAuth precisa estar dentro de <AuthProvider>.');
  return ctx;
}

/** Atalho para checar permissões em componentes. */
export function usePermissao(p: string): boolean {
  return useAuth().temPermissao(p);
}
