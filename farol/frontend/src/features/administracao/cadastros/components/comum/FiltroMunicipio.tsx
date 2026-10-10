import * as React from 'react';
import { NativeSelect } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import type { MunicipioDto } from '@/types/api';

/** Opções de município para <select> (inativos identificados). */
export function OpcoesMunicipio({ municipios }: { municipios: readonly MunicipioDto[] }) {
  return (
    <>
      {municipios.map((m) => (
        <option key={m.id} value={m.id}>
          {m.nome}
          {m.uf ? ` / ${m.uf}` : ''}
          {m.ativo ? '' : ' (inativo)'}
        </option>
      ))}
    </>
  );
}

/** Filtro de município de uma aba de cadastro. `permitirTodos` adiciona "Todos os municípios". */
export function FiltroMunicipio({
  id,
  municipios,
  value,
  onChange,
  permitirTodos,
  label = 'Município',
}: {
  id: string;
  municipios: readonly MunicipioDto[];
  value: number | null;
  onChange: (v: number | null) => void;
  permitirTodos?: boolean;
  label?: string;
}) {
  return (
    <div className="flex min-w-48 flex-col gap-1">
      <Label htmlFor={id}>{label}</Label>
      <NativeSelect
        id={id}
        value={value ?? ''}
        onChange={(e: React.ChangeEvent<HTMLSelectElement>) => onChange(e.target.value === '' ? null : Number(e.target.value))}
      >
        {permitirTodos ? <option value="">Todos os municípios</option> : value === null && <option value="">Selecione…</option>}
        <OpcoesMunicipio municipios={municipios} />
      </NativeSelect>
    </div>
  );
}

/** Nome do município pelo id. */
export function nomeMunicipio(municipios: readonly MunicipioDto[] | undefined, id: number | null | undefined): string {
  if (id === null || id === undefined) return '—';
  return municipios?.find((m) => m.id === id)?.nome ?? `#${id}`;
}
