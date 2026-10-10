import * as React from 'react';
import { cn } from '@/lib/utils';

/** Tabela densa e legível (13px), com cabeçalho fixo dentro do contêiner rolável. */
export const Table = React.forwardRef<HTMLTableElement, React.TableHTMLAttributes<HTMLTableElement> & { containerClassName?: string }>(
  ({ className, containerClassName, ...props }, ref) => (
    <div className={cn('relative w-full overflow-auto rounded-md border border-line bg-white', containerClassName)}>
      <table ref={ref} className={cn('w-full caption-bottom border-collapse text-[13px]', className)} {...props} />
    </div>
  ),
);
Table.displayName = 'Table';

export const THead = React.forwardRef<HTMLTableSectionElement, React.HTMLAttributes<HTMLTableSectionElement>>(
  ({ className, ...props }, ref) => <thead ref={ref} className={cn('sticky top-0 z-[1] bg-[#eef2f8]', className)} {...props} />,
);
THead.displayName = 'THead';

export const TBody = React.forwardRef<HTMLTableSectionElement, React.HTMLAttributes<HTMLTableSectionElement>>(
  ({ className, ...props }, ref) => <tbody ref={ref} className={cn('[&_tr:last-child]:border-0', className)} {...props} />,
);
TBody.displayName = 'TBody';

export const TR = React.forwardRef<HTMLTableRowElement, React.HTMLAttributes<HTMLTableRowElement>>(
  ({ className, ...props }, ref) => (
    <tr ref={ref} className={cn('border-b border-line hover:bg-[#f7f9fc] data-[selected=true]:bg-primary-soft', className)} {...props} />
  ),
);
TR.displayName = 'TR';

export const TH = React.forwardRef<HTMLTableCellElement, React.ThHTMLAttributes<HTMLTableCellElement>>(
  ({ className, ...props }, ref) => (
    <th
      ref={ref}
      scope="col"
      className={cn('h-8 whitespace-nowrap border-b border-line px-2 text-left align-middle text-xs font-semibold uppercase tracking-wide text-[#3d4f66]', className)}
      {...props}
    />
  ),
);
TH.displayName = 'TH';

export const TD = React.forwardRef<HTMLTableCellElement, React.TdHTMLAttributes<HTMLTableCellElement>>(
  ({ className, ...props }, ref) => <td ref={ref} className={cn('px-2 py-1.5 align-top', className)} {...props} />,
);
TD.displayName = 'TD';
