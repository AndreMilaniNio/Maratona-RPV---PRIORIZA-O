import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react';
import { Table, TBody, TH, THead, TR } from '@/components/ui/table';
import { direcaoOrdenacao, proximaOrdenacao, type CampoOrdenacao } from '@/features/ordens-servico/schemas/filtroFila';
import { cn } from '@/lib/utils';
import type { CatalogoDto, FilaItemDto } from '@/types/api';
import type { ColunaDef } from './colunas';
import { QueueRow } from './QueueRow';

export interface QueueTableProps {
  itens: FilaItemDto[];
  colunas: ColunaDef[];
  ordenarPor: string;
  onOrdenar: (ordenarPor: string) => void;
  agora: Date;
  selecionadaId: string | null;
  podeDespachar: boolean;
  catalogo?: CatalogoDto;
  onSelecionar: (id: string) => void;
  onAbrir: (id: string) => void;
  onDespachar: (id: string) => void;
  /** Atualizando em segundo plano (mantém os dados atuais visíveis). */
  atualizando?: boolean;
}

/** Tabela principal da fila operacional (seção 5.2). */
export function QueueTable({ itens, colunas, ordenarPor, onOrdenar, atualizando, ...linha }: QueueTableProps) {
  return (
    <Table containerClassName={cn('max-h-[70vh]', atualizando && 'opacity-80')} aria-label="Fila de ordens de serviço" aria-busy={atualizando}>
      <THead>
        <TR className="hover:bg-transparent">
          {colunas.map((c) => (
            <TH
              key={c.id}
              className={cn(c.className)}
              aria-sort={c.ordenacao ? direcaoOrdenacao(ordenarPor, c.ordenacao) : undefined}
            >
              {c.ordenacao ? <BotaoOrdenar coluna={c} campo={c.ordenacao} ordenarPor={ordenarPor} onOrdenar={onOrdenar} /> : c.rotulo}
            </TH>
          ))}
        </TR>
      </THead>
      <TBody>
        {itens.map((item) => (
          <QueueRow key={item.id} item={item} colunas={colunas} selecionada={item.id === linha.selecionadaId} {...linha} />
        ))}
      </TBody>
    </Table>
  );
}

function BotaoOrdenar({
  coluna,
  campo,
  ordenarPor,
  onOrdenar,
}: {
  coluna: ColunaDef;
  campo: CampoOrdenacao;
  ordenarPor: string;
  onOrdenar: (v: string) => void;
}) {
  const dir = direcaoOrdenacao(ordenarPor, campo);
  const Icone = dir === 'ascending' ? ArrowUp : dir === 'descending' ? ArrowDown : ArrowUpDown;
  const titulo = campo === 'fila' ? 'Ordenar pela política da fila' : `Ordenar por ${coluna.rotulo.toLowerCase()}`;
  return (
    <button
      type="button"
      onClick={() => onOrdenar(proximaOrdenacao(ordenarPor, campo))}
      title={titulo}
      className={cn(
        'inline-flex items-center gap-1 rounded-sm uppercase tracking-wide hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40',
        dir !== 'none' && 'text-primary',
      )}
    >
      {coluna.rotulo}
      <Icone className={cn('h-3.5 w-3.5', dir === 'none' && 'opacity-40')} aria-hidden />
    </button>
  );
}
