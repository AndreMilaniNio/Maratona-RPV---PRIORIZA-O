import * as React from 'react';
import { cn } from '@/lib/utils';

export function PageHeader({
  title,
  description,
  actions,
  className,
  icon,
}: {
  title: React.ReactNode;
  description?: React.ReactNode;
  actions?: React.ReactNode;
  className?: string;
  icon?: React.ReactNode;
}) {
  return (
    <div className={cn('mb-3 flex flex-wrap items-end justify-between gap-3', className)}>
      <div className="min-w-0">
        <h1 className="flex items-center gap-2 text-lg font-semibold text-navy [&_svg]:size-5">
          {icon}
          {title}
        </h1>
        {description && <p className="text-[13px] text-muted">{description}</p>}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  );
}

/** Seção de formulário/página com título e borda discreta. */
export function Section({
  title,
  description,
  children,
  className,
  actions,
  id,
  icon,
}: {
  title: React.ReactNode;
  description?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
  actions?: React.ReactNode;
  id?: string;
  icon?: React.ReactNode;
}) {
  return (
    <section id={id} className={cn('rounded-md border border-line bg-white', className)} aria-labelledby={id ? `${id}-titulo` : undefined}>
      <header className="flex flex-wrap items-center justify-between gap-2 border-b border-line px-4 py-2">
        <div className="min-w-0">
          <h2 id={id ? `${id}-titulo` : undefined} className="flex items-center gap-1.5 text-sm font-semibold text-navy [&_svg]:size-4">
            {icon}
            {title}
          </h2>
          {description && <p className="text-xs text-muted">{description}</p>}
        </div>
        {actions}
      </header>
      <div className="px-4 py-3">{children}</div>
    </section>
  );
}
