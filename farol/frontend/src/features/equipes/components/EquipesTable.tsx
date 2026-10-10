import { Link } from 'react-router';
import { Pencil } from 'lucide-react';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { DemoBadge } from '@/components/feedback/demo';
import { ROTAS } from '@/app/config/navigation';
import { STATUS_OS_LABEL } from '@/lib/labels';
import type { EquipeDto, ItemCodigoDto } from '@/types/api';
import { DisponibilidadeEquipe, StatusEquipeIndicador } from './StatusEquipeIndicador';
import { UltimaLocalizacao } from './OrigemLocalizacao';

function Itens({ itens, vazio = '—' }: { itens: ItemCodigoDto[]; vazio?: string }) {
  if (itens.length === 0) return <span className="text-muted">{vazio}</span>;
  return (
    <span className="flex max-w-[220px] flex-wrap gap-1">
      {itens.map((i) => (
        <Badge key={i.id} variant="outline" title={i.nome ?? undefined}>
          {i.codigo}
        </Badge>
      ))}
    </span>
  );
}

export function EquipesTable({
  equipes,
  equipeDoUsuarioId,
  onEditar,
}: {
  equipes: EquipeDto[];
  /** Destaca a equipe vinculada ao usuário (equipe de campo). */
  equipeDoUsuarioId?: number | null;
  /** Presente só para quem tem `equipes.administrar`. */
  onEditar?: (e: EquipeDto) => void;
}) {
  return (
    <Table containerClassName="max-h-[calc(100vh-260px)]" aria-label="Equipes">
      <THead>
        <TR>
          <TH>Código</TH>
          <TH>Nome</TH>
          <TH>Município</TH>
          <TH>Status</TH>
          <TH>Disponibilidade</TH>
          <TH className="text-right">Integr.</TH>
          <TH>Qualificações</TH>
          <TH>Recursos</TH>
          <TH>Última posição</TH>
          <TH>OS atribuídas</TH>
          <TH>Ativa</TH>
          {onEditar && <TH className="w-px"><span className="sr-only">Ações</span></TH>}
        </TR>
      </THead>
      <TBody>
        {equipes.map((e) => (
          <TR key={e.id} data-selected={e.id === equipeDoUsuarioId || undefined} className={e.ativa ? undefined : 'text-muted'}>
            <TD className="whitespace-nowrap">
              <Link to={ROTAS.equipe(e.id)} className="font-semibold text-primary hover:underline">
                {e.codigo}
              </Link>
              {e.id === equipeDoUsuarioId && (
                <Badge variant="moss" className="ml-1">
                  Minha equipe
                </Badge>
              )}
              {e.demonstrativa && <DemoBadge className="mt-1 flex w-fit" />}
            </TD>
            <TD className="min-w-[140px]">{e.nome}</TD>
            <TD className="whitespace-nowrap">
              {e.municipioBase}
              {e.municipiosAdicionais.length > 0 && (
                <span className="block text-xs text-muted" title="Municípios adicionais em que pode atuar">
                  + {e.municipiosAdicionais.map((m) => m.nome).join(', ')}
                </span>
              )}
            </TD>
            <TD>
              <StatusEquipeIndicador status={e.status} />
            </TD>
            <TD className="whitespace-nowrap">
              <DisponibilidadeEquipe equipe={e} />
            </TD>
            <TD className="text-right tabular-nums">{e.integrantes.length}</TD>
            <TD>
              <Itens itens={e.qualificacoes} />
            </TD>
            <TD>
              <Itens itens={e.recursos} />
            </TD>
            <TD className="whitespace-nowrap">
              <UltimaLocalizacao em={e.localizacaoEm} origem={e.origemLocalizacao} />
            </TD>
            <TD className="whitespace-nowrap">
              {e.osAtribuidas.length === 0 ? (
                <span className="text-muted">Nenhuma</span>
              ) : (
                <ul className="space-y-0.5">
                  {e.osAtribuidas.map((o) => (
                    <li key={o.osId}>
                      <Link to={ROTAS.os(o.osId)} className="font-medium text-primary hover:underline">
                        {o.numero ?? 'OS'}
                      </Link>
                      <span className="block text-xs text-muted">{STATUS_OS_LABEL[o.status] ?? o.status}</span>
                    </li>
                  ))}
                </ul>
              )}
            </TD>
            <TD>{e.ativa ? <Badge variant="success">Sim</Badge> : <Badge variant="neutral">Não</Badge>}</TD>
            {onEditar && (
              <TD>
                <Button size="icon-sm" variant="ghost" onClick={() => onEditar(e)} aria-label={`Editar equipe ${e.codigo}`} title="Editar">
                  <Pencil />
                </Button>
              </TD>
            )}
          </TR>
        ))}
      </TBody>
    </Table>
  );
}
