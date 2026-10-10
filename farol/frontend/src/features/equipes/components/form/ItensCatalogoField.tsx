import * as React from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { FieldGroup } from '@/components/forms/FormField';
import type { ItemCodigoDto } from '@/types/api';
import type { TipoItemEquipe } from '@/features/equipes/hooks/useEquipes';
import { ChecklistMultipla } from './ChecklistMultipla';
import { NovoItemInline } from './NovoItemInline';

/** Seleção de qualificações ou recursos do catálogo, com cadastro rápido de um novo item. */
export function ItensCatalogoField({
  tipo,
  legend,
  description,
  itens,
  value,
  onChange,
  error,
}: {
  tipo: TipoItemEquipe;
  legend: string;
  description?: string;
  itens: ItemCodigoDto[];
  value: number[];
  onChange: (ids: number[]) => void;
  error?: string;
}) {
  const [criando, setCriando] = React.useState(false);
  const opcoes = React.useMemo(
    () =>
      [...itens]
        .sort((a, b) => (a.nome ?? '').localeCompare(b.nome ?? '', 'pt-BR'))
        .map((i) => ({ id: i.id, rotulo: i.nome ?? i.codigo ?? String(i.id), descricao: i.codigo })),
    [itens],
  );
  return (
    <FieldGroup legend={legend} description={description} error={error}>
      <ChecklistMultipla idBase={`equipe-${tipo}`} opcoes={opcoes} value={value} onChange={onChange} />
      {criando ? (
        <NovoItemInline
          tipo={tipo}
          onCancelar={() => setCriando(false)}
          onCriado={(item) => {
            onChange(Array.from(new Set([...value, item.id])));
            setCriando(false);
          }}
        />
      ) : (
        <Button type="button" variant="link" size="sm" className="mt-1 px-0" onClick={() => setCriando(true)}>
          <Plus /> {tipo === 'qualificacao' ? 'Cadastrar nova qualificação' : 'Cadastrar novo recurso'}
        </Button>
      )}
    </FieldGroup>
  );
}
