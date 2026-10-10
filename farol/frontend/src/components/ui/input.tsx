import * as React from 'react';
import { cn } from '@/lib/utils';

export const inputClass =
  'h-8 w-full rounded-md border border-line bg-white px-2.5 text-[13px] text-ink placeholder:text-muted/70 focus-visible:outline-none focus-visible:border-primary focus-visible:ring-2 focus-visible:ring-primary/25 disabled:bg-canvas disabled:text-muted aria-[invalid=true]:border-danger';

export const Input = React.forwardRef<HTMLInputElement, React.InputHTMLAttributes<HTMLInputElement>>(
  ({ className, type, ...props }, ref) => (
    <input ref={ref} type={type ?? 'text'} className={cn(inputClass, className)} {...props} />
  ),
);
Input.displayName = 'Input';

export const Textarea = React.forwardRef<HTMLTextAreaElement, React.TextareaHTMLAttributes<HTMLTextAreaElement>>(
  ({ className, ...props }, ref) => (
    <textarea ref={ref} className={cn(inputClass, 'h-auto min-h-20 py-1.5 leading-snug', className)} {...props} />
  ),
);
Textarea.displayName = 'Textarea';

/** <select> nativo estilizado: acessível e eficiente em formulários densos. */
export const NativeSelect = React.forwardRef<HTMLSelectElement, React.SelectHTMLAttributes<HTMLSelectElement>>(
  ({ className, children, ...props }, ref) => (
    <select ref={ref} className={cn(inputClass, 'pr-7 cursor-pointer', className)} {...props}>
      {children}
    </select>
  ),
);
NativeSelect.displayName = 'NativeSelect';
