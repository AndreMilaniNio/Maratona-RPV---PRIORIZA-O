import { Search, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input, NativeSelect } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { STATUS_EQUIPE_LABEL } from '@/lib/labels';
import { StatusEquipeValores, type StatusEquipe } from '@/types/api';
import { FILTRO_EQUIPES_INICIAL, type FiltroDisponibilidade, type FiltroEquipes } from '@/features/equipes/utils/equipeExibicao';

export function EquipesFiltros({ valor, onChange }: { valor: FiltroEquipes; onChange: (f: FiltroEquipes) => void }) {
  const alterado = valor.busca !== '' || valor.status !== '' || valor.disponibilidade !== 'todas';
  return (
    <div className="flex flex-wrap items-end gap-3" role="search" aria-label="Filtrar equipes">
      <div className="flex min-w-[220px] flex-1 flex-col gap-1">
        <Label htmlFor="equipes-busca">Buscar</Label>
        <div className="relative">
          <Search className="pointer-events-none absolute left-2 top-1/2 size-4 -translate-y-1/2 text-muted" aria-hidden />
          <Input
            id="equipes-busca"
            className="pl-8"
            placeholder="Código, nome, integrante, qualificação, recurso ou OS"
            value={valor.busca}
            onChange={(e) => onChange({ ...valor, busca: e.target.value })}
          />
        </div>
      </div>
      <div className="flex w-52 flex-col gap-1">
        <Label htmlFor="equipes-status">Status</Label>
        <NativeSelect
          id="equipes-status"
          value={valor.status}
          onChange={(e) => onChange({ ...valor, status: e.target.value as StatusEquipe | '' })}
        >
          <option value="">Todos</option>
          {StatusEquipeValores.map((s) => (
            <option key={s} value={s}>
              {STATUS_EQUIPE_LABEL[s]}
            </option>
          ))}
        </NativeSelect>
      </div>
      <div className="flex w-48 flex-col gap-1">
        <Label htmlFor="equipes-disponibilidade">Disponibilidade</Label>
        <NativeSelect
          id="equipes-disponibilidade"
          value={valor.disponibilidade}
          onChange={(e) => onChange({ ...valor, disponibilidade: e.target.value as FiltroDisponibilidade })}
        >
          <option value="todas">Todas</option>
          <option value="disponiveis">Disponíveis para despacho</option>
          <option value="indisponiveis">Indisponíveis</option>
        </NativeSelect>
      </div>
      {alterado && (
        <Button variant="ghost" size="sm" onClick={() => onChange(FILTRO_EQUIPES_INICIAL)}>
          <X /> Limpar filtros
        </Button>
      )}
    </div>
  );
}
