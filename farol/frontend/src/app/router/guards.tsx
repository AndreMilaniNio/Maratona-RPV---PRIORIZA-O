import * as React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router';
import { ShieldOff } from 'lucide-react';
import { useAuth } from '@/features/autenticacao/hooks/AuthProvider';
import { LoadingState, EmptyState } from '@/components/feedback/states';
import type { RequisitoPermissao } from '@/lib/permissions';
import { ROTAS } from '@/app/config/navigation';

/** Exige sessão; sem sessão, vai ao Login guardando a rota de origem. */
export function RequireAuth() {
  const { estado } = useAuth();
  const location = useLocation();
  if (estado === 'anonimo') return <Navigate to={ROTAS.login} replace state={{ de: location.pathname }} />;
  if (estado === 'verificando') return <LoadingState label="Verificando sessão…" className="justify-center py-20" />;
  return <Outlet />;
}

/** Exige permissão para a rota (a API também valida). */
export function RequirePermission({ requisito, children }: { requisito?: RequisitoPermissao; children: React.ReactNode }) {
  const { atendeRequisito } = useAuth();
  if (!atendeRequisito(requisito)) return <AcessoNegado />;
  return <>{children}</>;
}

export function AcessoNegado() {
  return (
    <EmptyState
      icon={<ShieldOff />}
      title="Acesso não permitido"
      description="Seu perfil não tem permissão para esta tela. Se precisar de acesso, procure um administrador."
    />
  );
}
