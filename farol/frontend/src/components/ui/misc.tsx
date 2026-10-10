import * as React from 'react';
import { Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

export function Spinner({ className, label = 'Carregando' }: { className?: string; label?: string }) {
  return <Loader2 className={cn('h-4 w-4 animate-spin text-primary', className)} aria-label={label} role="status" />;
}

export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('animate-pulse rounded bg-[#e6ecf3]', className)} aria-hidden />;
}

/** Bloco com borda — use com moderação (não "cartões para tudo"). */
export function Panel({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('rounded-md border border-line bg-white', className)} {...props} />;
}

export function PanelHeader({
  title,
  icon,
  actions,
  className,
  description,
}: {
  title: React.ReactNode;
  icon?: React.ReactNode;
  actions?: React.ReactNode;
  className?: string;
  description?: React.ReactNode;
}) {
  return (
    <div className={cn('flex flex-wrap items-center justify-between gap-2 border-b border-line px-3 py-2', className)}>
      <div className="min-w-0">
        <h3 className="flex items-center gap-1.5 text-[13px] font-semibold uppercase tracking-wide text-navy [&_svg]:size-4">
          {icon}
          {title}
        </h3>
        {description && <p className="text-xs text-muted">{description}</p>}
      </div>
      {actions && <div className="flex items-center gap-2">{actions}</div>}
    </div>
  );
}

/** Lista de pares rótulo/valor em grade densa. */
export function DescriptionList({ className, children }: { className?: string; children: React.ReactNode }) {
  return <dl className={cn('grid grid-cols-[minmax(120px,auto)_1fr] gap-x-3 gap-y-1 text-[13px]', className)}>{children}</dl>;
}
export function DItem({ label, children }: { label: React.ReactNode; children: React.ReactNode }) {
  return (
    <>
      <dt className="text-muted">{label}</dt>
      <dd className="min-w-0 break-words">{children ?? '—'}</dd>
    </>
  );
}

export function Kbd({ children }: { children: React.ReactNode }) {
  return <kbd className="rounded border border-line bg-canvas px-1 font-mono text-[11px]">{children}</kbd>;
}
