import * as React from 'react';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { DemoBadge } from '@/components/feedback/demo';
import { InlineAlert } from '@/components/feedback/states';
import { notificarErro, notificarSucesso } from '@/components/feedback/toast';
import { ApiError } from '@/services/api/client';
import { formatCep, formatCoordenada, formatNumber } from '@/lib/format';
import type { MunicipioDto } from '@/types/api';
import { useExcluirMunicipio, useMunicipiosAdmin, useSalvarMunicipio } from '../../hooks/useCadastros';
import { municipioComAtivo } from '../../schemas/redeSchemas';
import { AtivoBadge, EstadoLista } from '../comum/Indicadores';
import { ConfirmDialog } from '../comum/ConfirmDialog';
import { MunicipioFormDialog } from './MunicipioFormDialog';

export function MunicipiosTab() {
  const consulta = useMunicipiosAdmin();
  const excluir = useExcluirMunicipio();
  const salvar = useSalvarMunicipio();
  const [editando, setEditando] = React.useState<MunicipioDto | null | undefined>(undefined);
  const [excluindo, setExcluindo] = React.useState<MunicipioDto | null>(null);
  const lista = consulta.data ?? [];

  const inativar = async (m: MunicipioDto) => {
    try {
      await salvar.mutateAsync({ id: m.id, dados: municipioComAtivo(m, false) });
      notificarSucesso('Município inativado.', m.nome ?? undefined);
      setExcluindo(null);
    } catch (e) {
      notificarErro(e, 'Não foi possível inativar o município');
    }
  };

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-[13px] text-muted">
          Município com OS vinculada não pode ser excluído — apenas inativado (seção 3A.1).
        </p>
        <Button onClick={() => setEditando(null)}>
          <Plus /> Novo município
        </Button>
      </div>
      <EstadoLista
        carregando={consulta.isLoading}
        erro={consulta.error}
        vazio={lista.length === 0}
        onRetry={() => consulta.refetch()}
        tituloVazio="Nenhum município cadastrado"
      >
        <Table>
          <THead>
            <TR>
              <TH>Nome</TH>
              <TH>UF</TH>
              <TH>IBGE</TH>
              <TH>Prefixo</TH>
              <TH>Coordenadas</TH>
              <TH className="text-right">Raio</TH>
              <TH>CEP único</TH>
              <TH>Situação</TH>
              <TH>
                <span className="sr-only">Ações</span>
              </TH>
            </TR>
          </THead>
          <TBody>
            {lista.map((m) => (
              <TR key={m.id}>
                <TD className="font-medium">
                  <span className="mr-1.5">{m.nome}</span>
                  <DemoBadge show={m.demonstrativo} />
                </TD>
                <TD>{m.uf}</TD>
                <TD className="font-mono text-xs">{m.codigoIbge ?? '—'}</TD>
                <TD className="font-mono">{m.prefixo}</TD>
                <TD className="whitespace-nowrap font-mono text-xs">{formatCoordenada(m.latitude, m.longitude)}</TD>
                <TD className="text-right whitespace-nowrap">{m.raioKm != null ? `${formatNumber(m.raioKm, 1)} km` : '—'}</TD>
                <TD className="font-mono text-xs">{m.cepUnico ? formatCep(m.cepUnico) : '—'}</TD>
                <TD>
                  <AtivoBadge ativo={m.ativo} />
                </TD>
                <TD className="whitespace-nowrap text-right">
                  <Button size="icon-sm" variant="ghost" aria-label={`Editar ${m.nome}`} title="Editar" onClick={() => setEditando(m)}>
                    <Pencil />
                  </Button>
                  <Button size="icon-sm" variant="ghost" aria-label={`Excluir ${m.nome}`} title="Excluir" onClick={() => setExcluindo(m)}>
                    <Trash2 className="text-danger" />
                  </Button>
                </TD>
              </TR>
            ))}
          </TBody>
        </Table>
      </EstadoLista>

      {editando !== undefined && <MunicipioFormDialog municipio={editando} onClose={() => setEditando(undefined)} />}
      {excluindo && (
        <ConfirmDialog
          title={`Excluir ${excluindo.nome}?`}
          description="A exclusão só é aceita para municípios sem OS, solicitações, rede ou equipes vinculadas."
          confirmLabel="Excluir"
          danger
          onClose={() => setExcluindo(null)}
          onConfirm={async () => {
            await excluir.mutateAsync(excluindo.id);
            notificarSucesso('Município excluído.', excluindo.nome ?? undefined);
          }}
        >
          {(erro) =>
            erro instanceof ApiError && erro.status === 409 ? (
              <InlineAlert tone="warning" title="Município com OS vinculada não pode ser excluído — apenas inativado.">
                <p className="mb-1 text-xs">Resposta da API: {erro.message}</p>
                <p className="mb-2">Inative o município para impedir novos registros, preservando o histórico.</p>
                {excluindo.ativo ? (
                  <Button size="sm" variant="secondary" loading={salvar.isPending} onClick={() => inativar(excluindo)}>
                    Inativar
                  </Button>
                ) : (
                  <p className="font-medium">Este município já está inativo.</p>
                )}
              </InlineAlert>
            ) : null
          }
        </ConfirmDialog>
      )}
    </div>
  );
}
