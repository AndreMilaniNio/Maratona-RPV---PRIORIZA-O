import * as React from 'react';
import { MapPinOff, MapPinned, ShieldAlert, Truck } from 'lucide-react';
import { TD, TR } from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tooltip } from '@/components/ui/tooltip';
import { PriorityBadge } from '@/features/priorizacao/components/PriorityBadge';
import { PrazoRestante } from '@/features/ordens-servico/components/PrazoRestante';
import { STATUS_DESPACHAVEIS } from '@/features/ordens-servico/schemas/filtroFila';
import { rotuloOpcao } from '@/hooks/useCatalogo';
import { formatDateTimeShort, formatNumber, formatPrazoRestante } from '@/lib/format';
import { FAIXA_QUANTIDADE_LABEL, STATUS_OS_LABEL, TIPO_MANUTENCAO_LABEL, TIPO_PRAZO_LABEL } from '@/lib/labels';
import { cn } from '@/lib/utils';
import type { CatalogoDto, FilaItemDto } from '@/types/api';
import type { ColunaDef, ColunaId } from './colunas';

export interface QueueRowProps {
  item: FilaItemDto;
  colunas: ColunaDef[];
  agora: Date;
  selecionada: boolean;
  /** Usuário tem `despacho.designar`. */
  podeDespachar: boolean;
  catalogo?: CatalogoDto;
  onSelecionar: (id: string) => void;
  onAbrir: (id: string) => void;
  onDespachar: (id: string) => void;
}

/** OS sem equipe ativa, em status despachável. */
export function osDespachavel(item: Pick<FilaItemDto, 'equipe' | 'status'>): boolean {
  return item.equipe == null && STATUS_DESPACHAVEIS.includes(item.status);
}

export function idLinhaFila(id: string) {
  return `fila-os-${id}`;
}

function Vazio({ children = '—' }: { children?: React.ReactNode }) {
  return <span className="text-muted">{children}</span>;
}

/** Uma linha da fila operacional. */
export function QueueRow({ item, colunas, agora, selecionada, podeDespachar, catalogo, onSelecionar, onAbrir, onDespachar }: QueueRowProps) {
  const vencido = formatPrazoRestante(item.proximoPrazo, agora).estado === 'vencido';
  const despachavel = podeDespachar && osDespachavel(item);

  const celula = (id: ColunaId): React.ReactNode => {
    switch (id) {
      case 'posicao':
        return <span className="font-semibold tabular-nums">{item.posicao ?? '—'}</span>;
      case 'numero':
        return (
          <button
            type="button"
            className="font-semibold whitespace-nowrap text-primary underline-offset-2 hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 rounded-sm"
            onClick={(e) => {
              e.stopPropagation();
              onSelecionar(item.id);
              onAbrir(item.id);
            }}
            aria-label={`Abrir detalhes da OS ${item.numero ?? ''}`}
          >
            {item.numero ?? '—'}
          </button>
        );
      case 'prioridade':
        return <PriorityBadge prioridade={item.prioridade} manual={item.prioridadeManual} />;
      case 'pontuacao':
        return <span className="tabular-nums">{formatNumber(item.pontuacao)}</span>;
      case 'manutencao':
        return TIPO_MANUTENCAO_LABEL[item.tipoManutencao] ?? item.tipoManutencao;
      case 'ocorrencia':
        return item.tipoOcorrencia ?? <Vazio />;
      case 'endereco':
        return (
          <div className="min-w-[14rem] max-w-[22rem]">
            <span className="line-clamp-2">{item.endereco ?? <Vazio>Endereço não informado</Vazio>}</span>
            {item.trecho && <span className="block text-xs text-muted">Trecho: {item.trecho}</span>}
            {(item.localizacaoPendente || !item.temCoordenadas) && (
              <span className="mt-0.5 flex flex-wrap gap-1">
                {item.localizacaoPendente && (
                  <Badge variant="warning">
                    <MapPinned aria-hidden /> Localização pendente
                  </Badge>
                )}
                {!item.temCoordenadas && (
                  <Badge variant="outline">
                    <MapPinOff aria-hidden /> Sem coordenadas
                  </Badge>
                )}
              </span>
            )}
          </div>
        );
      case 'municipio':
        return item.municipio ?? <Vazio />;
      case 'uc':
        return item.ucNaoInformada || item.ucs.length === 0 ? (
          <span className="italic text-muted">UC não informada</span>
        ) : (
          <span className="tabular-nums">{item.ucs.join(', ')}</span>
        );
      case 'classe':
        return item.classes.length ? item.classes.join(', ') : <Vazio />;
      case 'rede':
        return (
          <div className="text-xs leading-snug whitespace-nowrap">
            <div>Circ. {item.subestacao ?? '—'}</div>
            <div>Conj. {item.conjunto ?? '—'}</div>
            <div>Trafo {item.transformador ?? '—'}</div>
          </div>
        );
      case 'afetados':
        return (
          <div className="text-xs leading-snug whitespace-nowrap">
            <div>Pessoas: {item.faixaPessoas ? (FAIXA_QUANTIDADE_LABEL[item.faixaPessoas] ?? item.faixaPessoas) : '—'}</div>
            <div>UCs: {item.faixaUcs ? (FAIXA_QUANTIDADE_LABEL[item.faixaUcs] ?? item.faixaUcs) : '—'}</div>
          </div>
        );
      case 'risco':
        return <RiscoCelula item={item} catalogo={catalogo} />;
      case 'abertura':
        return <span className="whitespace-nowrap tabular-nums">{formatDateTimeShort(item.abertaEm)}</span>;
      case 'prazo':
        return item.proximoPrazo ? (
          <div className="whitespace-nowrap text-xs leading-snug">
            <div className="font-medium">{TIPO_PRAZO_LABEL[item.proximoPrazo.tipo] ?? item.proximoPrazo.tipo}</div>
            <div className="tabular-nums">{formatDateTimeShort(item.proximoPrazo.limite)}</div>
          </div>
        ) : (
          <Vazio>Sem prazo</Vazio>
        );
      case 'restante':
        return <PrazoRestante prazo={item.proximoPrazo} agora={agora} />;
      case 'status':
        return <span className="whitespace-nowrap">{STATUS_OS_LABEL[item.status] ?? item.status}</span>;
      case 'equipe':
        return item.equipe ? (
          <span className="whitespace-nowrap" title={item.equipe.nome ?? undefined}>
            {item.equipe.codigo ?? item.equipe.nome}
          </span>
        ) : (
          <Vazio>Sem equipe</Vazio>
        );
      case 'acoes':
        return (
          <div className="flex items-center gap-1" onClick={(e) => e.stopPropagation()}>
            {despachavel && (
              <Button size="sm" variant="primary" onClick={() => onDespachar(item.id)} aria-label={`Disponibilizar a OS ${item.numero ?? ''} para equipe`}>
                <Truck /> Disponibilizar para equipe
              </Button>
            )}
            <Button size="sm" variant="ghost" onClick={() => onAbrir(item.id)} aria-label={`Detalhes da OS ${item.numero ?? ''}`}>
              Detalhes
            </Button>
          </div>
        );
    }
  };

  return (
    <TR
      id={idLinhaFila(item.id)}
      data-selected={selecionada}
      data-vencido={vencido || undefined}
      onClick={() => onSelecionar(item.id)}
      className={cn('cursor-pointer', vencido && 'bg-danger-soft/50 hover:bg-danger-soft')}
    >
      {colunas.map((c, i) => (
        <TD
          key={c.id}
          className={cn(
            c.className?.includes('text-right') && 'text-right',
            i === 0 && vencido && 'shadow-[inset_3px_0_0_var(--color-danger)]',
            i === 0 && selecionada && !vencido && 'shadow-[inset_3px_0_0_var(--color-primary)]',
          )}
        >
          {celula(c.id)}
        </TD>
      ))}
    </TR>
  );
}

function RiscoCelula({ item, catalogo }: { item: FilaItemDto; catalogo?: CatalogoDto }) {
  if (!item.risco) return <Vazio>Sem risco</Vazio>;
  const condicoes = item.condicoesSeguranca.map((c) => rotuloOpcao(catalogo, 'RISCO_SEGURANCA', c));
  const texto = condicoes.length ? condicoes.join('; ') : 'Risco à segurança registrado';
  return (
    <Tooltip content={texto}>
      <span
        tabIndex={0}
        className="inline-flex items-center gap-1 rounded-sm font-semibold whitespace-nowrap text-danger focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40"
        aria-label={`Risco à segurança: ${texto}`}
      >
        <ShieldAlert className="h-3.5 w-3.5" aria-hidden />
        Risco
      </span>
    </Tooltip>
  );
}
