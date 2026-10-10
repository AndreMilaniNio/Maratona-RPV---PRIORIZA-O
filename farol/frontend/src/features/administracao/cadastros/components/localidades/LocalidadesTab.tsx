import * as React from 'react';
import { Pencil, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { DemoBadge } from '@/components/feedback/demo';
import type { LocalidadeDto } from '@/types/api';
import { useLocalidadesAdmin, useMunicipiosAdmin } from '../../hooks/useCadastros';
import { EstadoLista } from '../comum/Indicadores';
import { FiltroMunicipio } from '../comum/FiltroMunicipio';
import { LocalidadeFormDialog } from './LocalidadeFormDialog';

export function LocalidadesTab() {
  const municipios = useMunicipiosAdmin();
  const [municipioId, setMunicipioId] = React.useState<number | null>(null);
  const consulta = useLocalidadesAdmin(municipioId);
  const [editando, setEditando] = React.useState<LocalidadeDto | null | undefined>(undefined);
  const lista = consulta.data ?? [];

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <FiltroMunicipio id="loc-filtro" municipios={municipios.data ?? []} value={municipioId} onChange={setMunicipioId} permitirTodos />
        <Button onClick={() => setEditando(null)}>
          <Plus /> Nova localidade
        </Button>
      </div>
      <EstadoLista
        carregando={consulta.isLoading}
        erro={consulta.error}
        vazio={lista.length === 0}
        onRetry={() => consulta.refetch()}
        tituloVazio="Nenhuma localidade cadastrada"
        descricaoVazio="Cadastre os códigos de 3 dígitos usados no início do número dos transformadores."
      >
        <Table>
          <THead>
            <TR>
              <TH>Código</TH>
              <TH>Nome</TH>
              <TH>Município</TH>
              <TH>
                <span className="sr-only">Ações</span>
              </TH>
            </TR>
          </THead>
          <TBody>
            {lista.map((l) => (
              <TR key={l.id}>
                <TD>
                  <span className="rounded bg-primary-soft px-1.5 py-0.5 font-mono font-semibold text-primary">{l.codigo}</span>
                </TD>
                <TD>
                  <span className="mr-1.5">{l.nome}</span>
                  <DemoBadge show={l.demonstrativo} />
                </TD>
                <TD>{l.municipio ?? '—'}</TD>
                <TD className="text-right">
                  <Button size="icon-sm" variant="ghost" aria-label={`Editar localidade ${l.codigo}`} title="Editar" onClick={() => setEditando(l)}>
                    <Pencil />
                  </Button>
                </TD>
              </TR>
            ))}
          </TBody>
        </Table>
        <p className="text-xs text-muted">{lista.length} localidade(s). A API não oferece exclusão de localidades.</p>
      </EstadoLista>
      {editando !== undefined && (
        <LocalidadeFormDialog
          localidade={editando}
          municipioPadrao={municipioId}
          municipios={municipios.data ?? []}
          onClose={() => setEditando(undefined)}
        />
      )}
    </div>
  );
}
