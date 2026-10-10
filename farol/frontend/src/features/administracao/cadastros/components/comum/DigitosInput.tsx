import * as React from 'react';
import { Input } from '@/components/ui/input';
import { apenasDigitos } from '@/lib/utils';

/**
 * Campo de texto que aceita somente dígitos (código de localidade, número do transformador).
 * O valor continua texto: zeros à esquerda são preservados e nunca há conversão numérica.
 */
export const DigitosInput = React.forwardRef<
  HTMLInputElement,
  Omit<React.InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange'> & {
    value: string;
    onChange: (v: string) => void;
    maxDigitos?: number;
  }
>(({ value, onChange, maxDigitos, ...props }, ref) => (
  <Input
    ref={ref}
    {...props}
    inputMode="numeric"
    autoComplete="off"
    value={value}
    onChange={(e) => {
      const v = apenasDigitos(e.target.value);
      onChange(maxDigitos ? v.slice(0, maxDigitos) : v);
    }}
  />
));
DigitosInput.displayName = 'DigitosInput';
