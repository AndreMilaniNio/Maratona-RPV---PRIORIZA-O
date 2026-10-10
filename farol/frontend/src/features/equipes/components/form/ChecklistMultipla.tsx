import { CheckboxField } from '@/components/ui/checkbox';
import { cn } from '@/lib/utils';

export interface OpcaoChecklist {
  id: number;
  rotulo: string;
  descricao?: string | null;
  desabilitada?: boolean;
}

/** Lista de checkboxes para seleção múltipla de ids (municípios, qualificações, recursos). */
export function ChecklistMultipla({
  idBase,
  opcoes,
  value,
  onChange,
  colunas = 2,
  vazio = 'Nenhuma opção cadastrada.',
}: {
  idBase: string;
  opcoes: OpcaoChecklist[];
  value: number[];
  onChange: (ids: number[]) => void;
  colunas?: 1 | 2 | 3;
  vazio?: string;
}) {
  if (opcoes.length === 0) return <p className="text-xs text-muted">{vazio}</p>;
  const alternar = (id: number, marcado: boolean) =>
    onChange(marcado ? Array.from(new Set([...value, id])) : value.filter((v) => v !== id));
  return (
    <div
      className={cn(
        'grid gap-x-4 gap-y-1.5 rounded-md border border-line bg-white px-3 py-2',
        colunas === 1 ? 'grid-cols-1' : colunas === 2 ? 'grid-cols-1 sm:grid-cols-2' : 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3',
      )}
    >
      {opcoes.map((o) => (
        <CheckboxField
          key={o.id}
          id={`${idBase}-${o.id}`}
          label={o.rotulo}
          description={o.descricao ?? undefined}
          checked={value.includes(o.id)}
          disabled={o.desabilitada}
          onCheckedChange={(v) => alternar(o.id, v)}
        />
      ))}
    </div>
  );
}
