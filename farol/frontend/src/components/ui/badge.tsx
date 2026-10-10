import * as React from 'react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

export const badgeVariants = cva(
  'inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-xs font-medium leading-none whitespace-nowrap [&_svg]:size-3.5 [&_svg]:shrink-0',
  {
    variants: {
      variant: {
        neutral: 'bg-canvas text-ink border border-line',
        primary: 'bg-primary-soft text-primary border border-primary/20',
        navy: 'bg-navy text-white',
        moss: 'bg-moss-soft text-moss border border-moss/30',
        success: 'bg-success-soft text-success border border-success/25',
        warning: 'bg-warning-soft text-warning border border-warning/30',
        danger: 'bg-danger-soft text-danger border border-danger/25',
        demo: 'bg-amber-banner text-[#7a5a00] border border-amber-border',
        outline: 'border border-line text-muted',
      },
    },
    defaultVariants: { variant: 'neutral' },
  },
);

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement>, VariantProps<typeof badgeVariants> {}

export function Badge({ className, variant, ...props }: BadgeProps) {
  return <span className={cn(badgeVariants({ variant }), className)} {...props} />;
}
