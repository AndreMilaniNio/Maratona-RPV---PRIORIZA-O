import * as React from 'react';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';

/** Campo de formulário: rótulo, controle, dica e mensagem de erro (associada por aria-describedby). */
export function FormField({
  id,
  label,
  required,
  error,
  hint,
  className,
  children,
  labelExtra,
}: {
  id: string;
  label: React.ReactNode;
  required?: boolean;
  error?: string | string[] | null;
  hint?: React.ReactNode;
  className?: string;
  children: React.ReactElement<Record<string, unknown>>;
  labelExtra?: React.ReactNode;
}) {
  const msg = Array.isArray(error) ? error.join(' ') : error;
  const describedBy = [hint ? `${id}-hint` : null, msg ? `${id}-error` : null].filter(Boolean).join(' ') || undefined;
  const child = React.cloneElement(children, {
    id,
    'aria-invalid': msg ? true : undefined,
    'aria-describedby': describedBy,
    'aria-required': required || undefined,
  });
  return (
    <div className={cn('flex min-w-0 flex-col gap-1', className)}>
      <div className="flex items-center justify-between gap-2">
        <Label htmlFor={id}>
          {label}
          {required && (
            <span className="ml-0.5 text-danger" aria-hidden>
              *
            </span>
          )}
        </Label>
        {labelExtra}
      </div>
      {child}
      {hint && !msg && (
        <p id={`${id}-hint`} className="text-xs text-muted">
          {hint}
        </p>
      )}
      {msg && (
        <p id={`${id}-error`} className="text-xs font-medium text-danger" role="alert">
          {msg}
        </p>
      )}
    </div>
  );
}

/** Grupo de campos com legenda (fieldset). */
export function FieldGroup({
  legend,
  children,
  className,
  description,
  error,
}: {
  legend: React.ReactNode;
  children: React.ReactNode;
  className?: string;
  description?: React.ReactNode;
  error?: string | null;
}) {
  return (
    <fieldset className={cn('min-w-0', className)}>
      <legend className="mb-1 text-[13px] font-medium text-ink">{legend}</legend>
      {description && <p className="-mt-0.5 mb-1.5 text-xs text-muted">{description}</p>}
      {children}
      {error && (
        <p className="mt-1 text-xs font-medium text-danger" role="alert">
          {error}
        </p>
      )}
    </fieldset>
  );
}
