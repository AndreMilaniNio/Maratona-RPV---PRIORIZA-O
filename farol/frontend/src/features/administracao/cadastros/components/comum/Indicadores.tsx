import * as React from 'react';
import { Check, Minus } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { ErrorState, EmptyState, LoadingState } from '@/components/feedback/states';

/** "Sim"/"Não" com ícone (não depende só de cor). */
export function SimNao({ valor }: { valor: boolean | null | undefined }) {
  return valor ? (
    <span className="inline-flex items-center gap-1 text-ink">
      <Check className="h-3.5 w-3.5 text-success" aria-hidden />
      Sim
    </span>
  ) : (
    <span className="inline-flex items-center gap-1 text-muted">
      <Minus className="h-3.5 w-3.5" aria-hidden />
      Não
    </span>
  );
}

export function AtivoBadge({ ativo, feminino }: { ativo: boolean; feminino?: boolean }) {
  return ativo ? (
    <Badge variant="success">{feminino ? 'Ativa' : 'Ativo'}</Badge>
  ) : (
    <Badge variant="outline">{feminino ? 'Inativa' : 'Inativo'}</Badge>
  );
}

/**
 * Estados padrão de uma consulta de lista: carregando, erro (com retentar) e vazio.
 * Renderiza `children` quando há dados.
 */
export function EstadoLista({
  carregando,
  erro,
  vazio,
  onRetry,
  tituloVazio,
  descricaoVazio,
  acaoVazio,
  children,
}: {
  carregando: boolean;
  erro: unknown;
  vazio: boolean;
  onRetry?: () => void;
  tituloVazio: string;
  descricaoVazio?: React.ReactNode;
  acaoVazio?: React.ReactNode;
  children: React.ReactNode;
}) {
  if (carregando) return <LoadingState />;
  if (erro) return <ErrorState error={erro} onRetry={onRetry} title="Não foi possível carregar" />;
  if (vazio) return <EmptyState title={tituloVazio} description={descricaoVazio} action={acaoVazio} />;
  return <>{children}</>;
}
