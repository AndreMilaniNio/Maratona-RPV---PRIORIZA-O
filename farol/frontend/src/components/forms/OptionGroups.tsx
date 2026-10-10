import { HelpCircle } from 'lucide-react';
import { Checkbox } from '@/components/ui/checkbox';
import { cn } from '@/lib/utils';

export interface Opcao {
  codigo: string;
  rotulo: string;
  /** Opção que representa "desconhecido / não informado" — destacada visualmente. */
  representaDesconhecido?: boolean;
}

/**
 * Grupo de rádio para uma opção de critério. "Desconhecido" é uma escolha explícita,
 * destacada com ícone — nunca tratada como zero.
 */
export function OptionRadioGroup({
  name,
  opcoes,
  value,
  onChange,
  disabled,
  columns = 3,
  ariaLabel,
}: {
  name: string;
  opcoes: Opcao[];
  value: string | null | undefined;
  onChange: (codigo: string) => void;
  disabled?: boolean;
  columns?: 1 | 2 | 3 | 4;
  ariaLabel?: string;
}) {
  const cols = { 1: 'grid-cols-1', 2: 'sm:grid-cols-2', 3: 'sm:grid-cols-2 lg:grid-cols-3', 4: 'sm:grid-cols-2 lg:grid-cols-4' }[columns];
  return (
    <div role="radiogroup" aria-label={ariaLabel} className={cn('grid gap-1', cols)}>
      {opcoes.map((o) => {
        const id = `${name}-${o.codigo}`;
        const checked = value === o.codigo;
        return (
          <label
            key={o.codigo}
            htmlFor={id}
            className={cn(
              'flex cursor-pointer items-center gap-2 rounded border px-2 py-1 text-[13px]',
              checked ? 'border-primary bg-primary-soft' : 'border-line bg-white hover:bg-canvas',
              o.representaDesconhecido && !checked && 'border-dashed',
              disabled && 'cursor-not-allowed opacity-60',
            )}
          >
            <input
              id={id}
              type="radio"
              name={name}
              value={o.codigo}
              checked={checked}
              disabled={disabled}
              onChange={() => onChange(o.codigo)}
              className="h-3.5 w-3.5 accent-[#2457A6]"
            />
            <span className="flex-1">{o.rotulo}</span>
            {o.representaDesconhecido && <HelpCircle className="h-3.5 w-3.5 text-muted" aria-label="opção de informação desconhecida" />}
          </label>
        );
      })}
    </div>
  );
}

/**
 * Grupo de múltipla escolha. Códigos em `exclusivos` (ex.: SEM_RISCO_ADICIONAL, DESCONHECIDA)
 * desmarcam as demais opções quando escolhidos, e vice-versa.
 */
export function OptionCheckboxGroup({
  name,
  opcoes,
  value,
  onChange,
  exclusivos = [],
  disabled,
  columns = 2,
}: {
  name: string;
  opcoes: Opcao[];
  value: string[];
  onChange: (codigos: string[]) => void;
  exclusivos?: string[];
  disabled?: boolean;
  columns?: 1 | 2 | 3;
}) {
  const cols = { 1: 'grid-cols-1', 2: 'sm:grid-cols-2', 3: 'sm:grid-cols-2 lg:grid-cols-3' }[columns];
  const toggle = (codigo: string, marcar: boolean) => {
    if (!marcar) return onChange(value.filter((v) => v !== codigo));
    if (exclusivos.includes(codigo)) return onChange([codigo]);
    return onChange([...value.filter((v) => !exclusivos.includes(v)), codigo]);
  };
  return (
    <div className={cn('grid gap-1', cols)} role="group">
      {opcoes.map((o) => {
        const id = `${name}-${o.codigo}`;
        const checked = value.includes(o.codigo);
        return (
          <div
            key={o.codigo}
            className={cn(
              'flex items-center gap-2 rounded border px-2 py-1',
              checked ? 'border-primary bg-primary-soft' : 'border-line bg-white',
              o.representaDesconhecido && !checked && 'border-dashed',
            )}
          >
            <Checkbox id={id} checked={checked} disabled={disabled} onCheckedChange={(v) => toggle(o.codigo, v === true)} />
            <label htmlFor={id} className="flex flex-1 cursor-pointer items-center gap-1 text-[13px]">
              {o.rotulo}
              {exclusivos.includes(o.codigo) && <span className="text-xs text-muted">(exclusiva)</span>}
            </label>
            {o.representaDesconhecido && <HelpCircle className="h-3.5 w-3.5 text-muted" aria-label="opção de informação desconhecida" />}
          </div>
        );
      })}
    </div>
  );
}
