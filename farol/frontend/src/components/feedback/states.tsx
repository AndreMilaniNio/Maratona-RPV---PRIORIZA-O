import * as React from 'react';
import { AlertTriangle, CircleAlert, Inbox, RefreshCw, WifiOff } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Spinner } from '@/components/ui/misc';
import { ApiError, mensagemErro } from '@/services/api/client';
import { cn } from '@/lib/utils';

export function LoadingState({ label = 'Carregando…', className }: { label?: string; className?: string }) {
  return (
    <div className={cn('flex items-center gap-2 px-3 py-6 text-[13px] text-muted', className)} role="status" aria-live="polite">
      <Spinner />
      {label}
    </div>
  );
}

export function EmptyState({
  title,
  description,
  action,
  icon,
  className,
}: {
  title: string;
  description?: React.ReactNode;
  action?: React.ReactNode;
  icon?: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={cn('flex flex-col items-center justify-center gap-2 px-4 py-10 text-center', className)}>
      <span className="text-muted [&_svg]:size-7">{icon ?? <Inbox />}</span>
      <p className="font-medium text-ink">{title}</p>
      {description && <p className="max-w-md text-[13px] text-muted">{description}</p>}
      {action}
    </div>
  );
}

/** Lista as mensagens de um ApiError (título, erros por campo, pendências). */
export function ProblemDetailsMessages({ error }: { error: unknown }) {
  if (!(error instanceof ApiError)) return <p>{mensagemErro(error)}</p>;
  const campos = Object.entries(error.fieldErrors);
  const pend = error.pendencias;
  return (
    <div className="space-y-1">
      <p className="font-medium">{error.message}</p>
      {error.problem?.detail && error.problem.detail !== error.message && <p>{error.problem.detail}</p>}
      {campos.length > 0 && (
        <ul className="list-disc pl-5">
          {campos.map(([campo, msgs]) => (
            <li key={campo}>
              {campo ? <span className="font-mono text-xs">{campo}</span> : null}
              {campo ? ': ' : ''}
              {msgs.join(' ')}
            </li>
          ))}
        </ul>
      )}
      {pend.length > 0 && (
        <>
          <p className="font-medium">Pendências:</p>
          <ul className="list-disc pl-5">
            {pend.map((p, i) => (
              <li key={i}>{p}</li>
            ))}
          </ul>
        </>
      )}
    </div>
  );
}

export function ErrorState({
  error,
  onRetry,
  className,
  title,
  compact,
}: {
  error: unknown;
  onRetry?: () => void;
  className?: string;
  title?: string;
  compact?: boolean;
}) {
  const network = error instanceof ApiError && error.isNetwork;
  const forbidden = error instanceof ApiError && error.status === 403;
  return (
    <div
      role="alert"
      className={cn(
        'flex items-start gap-2 rounded-md border border-danger/30 bg-danger-soft text-[13px] text-[#7a1a12]',
        compact ? 'px-2.5 py-2' : 'px-3 py-3',
        className,
      )}
    >
      {network ? <WifiOff className="mt-0.5 h-4 w-4 shrink-0" /> : <CircleAlert className="mt-0.5 h-4 w-4 shrink-0" />}
      <div className="min-w-0 flex-1">
        {title && <p className="font-semibold">{title}</p>}
        {forbidden ? <p>Você não tem permissão para acessar estes dados.</p> : <ProblemDetailsMessages error={error} />}
      </div>
      {onRetry && (
        <Button size="sm" variant="secondary" onClick={onRetry}>
          <RefreshCw /> Tentar novamente
        </Button>
      )}
    </div>
  );
}

/** Aviso de dados possivelmente desatualizados (backend indisponível durante a atualização). */
export function StaleDataNotice({ onRetry, retrying, className }: { onRetry: () => void; retrying?: boolean; className?: string }) {
  return (
    <div role="status" className={cn('flex flex-wrap items-center gap-2 rounded-md border border-warning/40 bg-warning-soft px-3 py-1.5 text-[13px] text-[#7a3306]', className)}>
      <AlertTriangle className="h-4 w-4" />
      <span>Não foi possível atualizar agora — os dados exibidos podem estar desatualizados.</span>
      <Button size="sm" variant="secondary" onClick={onRetry} loading={retrying}>
        <RefreshCw /> Tentar novamente
      </Button>
    </div>
  );
}

/** Caixa de alerta genérica (aviso não bloqueante). */
export function InlineAlert({
  tone = 'warning',
  title,
  children,
  className,
  icon,
}: {
  tone?: 'warning' | 'info' | 'danger' | 'success';
  title?: React.ReactNode;
  children?: React.ReactNode;
  className?: string;
  icon?: React.ReactNode;
}) {
  const tones = {
    warning: 'border-warning/35 bg-warning-soft text-[#7a3306]',
    info: 'border-primary/25 bg-primary-soft text-navy',
    danger: 'border-danger/30 bg-danger-soft text-[#7a1a12]',
    success: 'border-success/30 bg-success-soft text-success',
  } as const;
  return (
    <div role={tone === 'danger' ? 'alert' : 'status'} className={cn('flex gap-2 rounded-md border px-3 py-2 text-[13px]', tones[tone], className)}>
      <span className="mt-0.5 shrink-0 [&_svg]:size-4">{icon ?? <AlertTriangle />}</span>
      <div className="min-w-0">
        {title && <p className="font-semibold">{title}</p>}
        {children}
      </div>
    </div>
  );
}
