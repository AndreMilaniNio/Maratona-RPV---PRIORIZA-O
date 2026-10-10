import * as React from 'react';
import { ChevronRight, OctagonAlert, TimerOff } from 'lucide-react';
import { Skeleton } from '@/components/ui/misc';
import { ErrorState } from '@/components/feedback/states';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { formatNumber } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { IndicadoresDto } from '@/types/api';

export type ChaveIndicador =
  | 'abertas'
  | 'criticas'
  | 'aguardandoDespacho'
  | 'emAtendimento'
  | 'vencidas'
  | 'equipesDisponiveis'
  | 'equipesDeslocamento'
  | 'equipesExecutando';

interface DefIndicador {
  chave: ChaveIndicador;
  rotulo: string;
  /** Ação ao clicar (descrição para leitores de tela). */
  filtro?: string;
  tom?: 'danger' | 'warning';
  icone?: React.ReactNode;
}

const OS: DefIndicador[] = [
  { chave: 'abertas', rotulo: 'OS abertas', filtro: 'Mostrar a fila padrão' },
  { chave: 'criticas', rotulo: 'Críticas / urgentes', filtro: 'Filtrar prioridades críticas', tom: 'danger', icone: <OctagonAlert /> },
  { chave: 'aguardandoDespacho', rotulo: 'Aguardando despacho', filtro: 'Filtrar OS aguardando despacho' },
  { chave: 'emAtendimento', rotulo: 'Em atendimento', filtro: 'Filtrar OS em atendimento' },
  { chave: 'vencidas', rotulo: 'Prazo vencido', filtro: 'Filtrar OS com prazo vencido', tom: 'danger', icone: <TimerOff /> },
];
const EQUIPES: DefIndicador[] = [
  { chave: 'equipesDisponiveis', rotulo: 'Disponíveis' },
  { chave: 'equipesDeslocamento', rotulo: 'Em deslocamento' },
  { chave: 'equipesExecutando', rotulo: 'Executando' },
];

export interface IndicatorsRowProps {
  dados: IndicadoresDto | undefined;
  carregando?: boolean;
  erro?: unknown;
  onRetry?: () => void;
  /** Visão "Todas as cidades": exibe o detalhamento por cidade. */
  mostrarPorCidade?: boolean;
  /** Clique em um indicador de OS (aplica o filtro correspondente). */
  onSelecionar?: (chave: ChaveIndicador) => void;
  ativo?: ChaveIndicador | null;
  className?: string;
}

/** Faixa compacta com os oito indicadores da seção 5.1. */
export function IndicatorsRow({ dados, carregando, erro, onRetry, mostrarPorCidade, onSelecionar, ativo, className }: IndicatorsRowProps) {
  if (!dados && erro) return <ErrorState error={erro} onRetry={onRetry} title="Não foi possível carregar os indicadores" compact className={className} />;

  return (
    <section aria-label="Indicadores" className={cn('rounded-md border border-line bg-white', className)}>
      <div className="flex flex-col divide-y divide-line lg:flex-row lg:divide-x lg:divide-y-0">
        <Grupo titulo="Ordens de serviço" itens={OS} dados={dados} carregando={carregando} onSelecionar={onSelecionar} ativo={ativo} className="lg:flex-[5]" />
        <Grupo titulo="Equipes" itens={EQUIPES} dados={dados} carregando={carregando} className="lg:flex-[3]" />
      </div>
      {mostrarPorCidade && dados && dados.porMunicipio.length > 0 && <PorCidade dados={dados} />}
    </section>
  );
}

function Grupo({
  titulo,
  itens,
  dados,
  carregando,
  onSelecionar,
  ativo,
  className,
}: {
  titulo: string;
  itens: DefIndicador[];
  dados: IndicadoresDto | undefined;
  carregando?: boolean;
  onSelecionar?: (chave: ChaveIndicador) => void;
  ativo?: ChaveIndicador | null;
  className?: string;
}) {
  return (
    <div className={cn('min-w-0 px-3 py-2', className)}>
      <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-muted">{titulo}</p>
      <ul className={cn('grid gap-x-2 gap-y-2', itens.length > 3 ? 'grid-cols-3 sm:grid-cols-5' : 'grid-cols-3')}>
        {itens.map((d) => {
          const valor = dados?.[d.chave];
          const destaque = d.tom && (valor ?? 0) > 0;
          const conteudo = (
            <>
              <span className="block truncate text-xs text-muted">{d.rotulo}</span>
              <span
                className={cn(
                  'flex items-center gap-1 text-xl font-semibold tabular-nums leading-tight text-navy [&_svg]:size-4',
                  destaque && d.tom === 'danger' && 'text-danger',
                  destaque && d.tom === 'warning' && 'text-warning',
                )}
                data-testid={`indicador-${d.chave}`}
              >
                {carregando && valor === undefined ? <Skeleton className="h-6 w-10" /> : formatNumber(valor)}
                {destaque && d.icone}
              </span>
            </>
          );
          if (!onSelecionar || !d.filtro)
            return (
              <li key={d.chave} className="min-w-0 px-1 py-0.5">
                {conteudo}
              </li>
            );
          return (
            <li key={d.chave} className="min-w-0">
              <button
                type="button"
                onClick={() => onSelecionar(d.chave)}
                title={d.filtro}
                aria-pressed={ativo === d.chave}
                className={cn(
                  'w-full min-w-0 rounded px-1 py-0.5 text-left hover:bg-primary-soft focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40',
                  ativo === d.chave && 'bg-primary-soft ring-1 ring-primary/30',
                )}
              >
                {conteudo}
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
}

function PorCidade({ dados }: { dados: IndicadoresDto }) {
  return (
    <details className="group border-t border-line">
      <summary className="flex cursor-pointer list-none items-center gap-1 px-3 py-1.5 text-[13px] font-medium text-primary hover:bg-canvas focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40">
        <ChevronRight className="h-4 w-4 transition-transform group-open:rotate-90" aria-hidden />
        Indicadores por cidade ({dados.porMunicipio.length})
      </summary>
      <div className="px-3 pb-3">
        <Table containerClassName="max-h-72">
          <THead>
            <TR>
              <TH>Cidade</TH>
              <TH className="text-right">Abertas</TH>
              <TH className="text-right">Críticas</TH>
              <TH className="text-right">Aguard. despacho</TH>
              <TH className="text-right">Em atendimento</TH>
              <TH className="text-right">Vencidas</TH>
              <TH className="text-right">Eq. disponíveis</TH>
              <TH className="text-right">Eq. deslocamento</TH>
              <TH className="text-right">Eq. executando</TH>
            </TR>
          </THead>
          <TBody>
            {dados.porMunicipio.map((c) => (
              <TR key={c.municipioId}>
                <TD className="font-medium">{c.municipio}</TD>
                <TD className="text-right tabular-nums">{c.abertas}</TD>
                <TD className={cn('text-right tabular-nums', c.criticas > 0 && 'font-semibold text-danger')}>{c.criticas}</TD>
                <TD className="text-right tabular-nums">{c.aguardandoDespacho}</TD>
                <TD className="text-right tabular-nums">{c.emAtendimento}</TD>
                <TD className={cn('text-right tabular-nums', c.vencidas > 0 && 'font-semibold text-danger')}>{c.vencidas}</TD>
                <TD className="text-right tabular-nums">{c.equipesDisponiveis}</TD>
                <TD className="text-right tabular-nums">{c.equipesDeslocamento}</TD>
                <TD className="text-right tabular-nums">{c.equipesExecutando}</TD>
              </TR>
            ))}
          </TBody>
        </Table>
      </div>
    </details>
  );
}
