import * as React from 'react';

/**
 * Valor "atrasado": só muda depois de `ms` sem alterações.
 * Use com valores primitivos (ex.: JSON serializado) para não reiniciar a cada render.
 */
export function useDebouncedValue<T extends string | number | boolean | null>(value: T, ms: number): T {
  const [v, setV] = React.useState(value);
  React.useEffect(() => {
    const t = setTimeout(() => setV(value), ms);
    return () => clearTimeout(t);
  }, [value, ms]);
  return v;
}
