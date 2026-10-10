import * as React from 'react';
import { ChevronDown } from 'lucide-react';
import { Label } from '@/components/ui/label';
import { Input, NativeSelect, inputClass } from '@/components/ui/input';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { CheckboxField } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export interface OpcaoFiltro {
  valor: string;
  rotulo: string;
}

function Campo({ id, label, children, className }: { id: string; label: string; children: React.ReactNode; className?: string }) {
  return (
    <div className={cn('flex min-w-0 flex-col gap-1', className)}>
      <Label htmlFor={id} className="text-xs text-muted">
        {label}
      </Label>
      {children}
    </div>
  );
}

/** Campo de texto aplicado com atraso (evita uma consulta por tecla). */
export function TextFilter({
  id,
  label,
  value,
  onChange,
  placeholder,
  className,
  atrasoMs = 450,
}: {
  id: string;
  label: string;
  value: string;
  onChange: (v: string) => void;
  placeholder?: string;
  className?: string;
  atrasoMs?: number;
}) {
  const [local, setLocal] = React.useState(value);
  React.useEffect(() => setLocal(value), [value]);
  const onChangeRef = React.useRef(onChange);
  onChangeRef.current = onChange;
  React.useEffect(() => {
    if (local.trim() === value) return;
    const t = setTimeout(() => onChangeRef.current(local.trim()), atrasoMs);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [local]);
  return (
    <Campo id={id} label={label} className={className}>
      <Input
        id={id}
        value={local}
        placeholder={placeholder}
        onChange={(e) => setLocal(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === 'Enter') onChangeRef.current(local.trim());
        }}
      />
    </Campo>
  );
}

/** Select nativo com opção "Todos" (valor vazio). */
export function SelectFilter({
  id,
  label,
  value,
  onChange,
  opcoes,
  todos = 'Todos',
  disabled,
  className,
}: {
  id: string;
  label: string;
  value: string;
  onChange: (v: string) => void;
  opcoes: OpcaoFiltro[];
  todos?: string;
  disabled?: boolean;
  className?: string;
}) {
  return (
    <Campo id={id} label={label} className={className}>
      <NativeSelect id={id} value={value} onChange={(e) => onChange(e.target.value)} disabled={disabled}>
        <option value="">{todos}</option>
        {opcoes.map((o) => (
          <option key={o.valor} value={o.valor}>
            {o.rotulo}
          </option>
        ))}
      </NativeSelect>
    </Campo>
  );
}

/** Seleção múltipla compacta (popover com caixas de seleção). */
export function MultiSelectFilter({
  id,
  label,
  value,
  onChange,
  opcoes,
  todos = 'Todos',
  className,
}: {
  id: string;
  label: string;
  value: string[];
  onChange: (v: string[]) => void;
  opcoes: OpcaoFiltro[];
  todos?: string;
  className?: string;
}) {
  const selecionados = opcoes.filter((o) => value.includes(o.valor));
  const resumo =
    selecionados.length === 0 ? todos : selecionados.length <= 2 ? selecionados.map((o) => o.rotulo).join(', ') : `${selecionados.length} selecionados`;
  return (
    <Campo id={id} label={label} className={className}>
      <Popover>
        <PopoverTrigger asChild>
          <button id={id} type="button" className={cn(inputClass, 'flex items-center justify-between gap-1 text-left')}>
            <span className={cn('truncate', selecionados.length === 0 && 'text-muted')}>{resumo}</span>
            <ChevronDown className="h-4 w-4 shrink-0 text-muted" aria-hidden />
          </button>
        </PopoverTrigger>
        <PopoverContent className="w-64 p-2" aria-label={label}>
          <div className="max-h-72 space-y-1.5 overflow-auto">
            {opcoes.map((o) => (
              <CheckboxField
                key={o.valor}
                id={`${id}-${o.valor}`}
                label={o.rotulo}
                checked={value.includes(o.valor)}
                onCheckedChange={(v) => onChange(v ? [...value, o.valor] : value.filter((x) => x !== o.valor))}
              />
            ))}
          </div>
          {value.length > 0 && (
            <Button variant="link" size="sm" className="mt-2" onClick={() => onChange([])}>
              Limpar seleção
            </Button>
          )}
        </PopoverContent>
      </Popover>
    </Campo>
  );
}

/** Campo de data (yyyy-mm-dd). */
export function DateFilter({ id, label, value, onChange }: { id: string; label: string; value: string; onChange: (v: string) => void }) {
  return (
    <Campo id={id} label={label}>
      <Input id={id} type="date" value={value} onChange={(e) => onChange(e.target.value)} />
    </Campo>
  );
}
