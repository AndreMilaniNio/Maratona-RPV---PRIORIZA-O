import * as React from 'react';
import { useQuery } from '@tanstack/react-query';
import { History, RefreshCw, Search, X } from 'lucide-react';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Pagination } from '@/components/tables/Pagination';
import { EmptyState, ErrorState, LoadingState } from '@/components/feedback/states';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { formatDateTime } from '@/lib/format';
import { api } from '@/services/api/client';
import { queryKeys } from '@/services/api/queryKeys';
import type { AuditoriaDtoPaginaDto } from '@/types/api';

type Filtros = { entidade: string; entidadeId: string; acao: string; de: string; ate: string; pagina: number; tamanhoPagina: number };
const inicial: Filtros = { entidade: '', entidadeId: '', acao: '', de: '', ate: '', pagina: 1, tamanhoPagina: 50 };

function paraIso(valor: string) {
  if (!valor) return undefined;
  const data = new Date(valor);
  return Number.isNaN(data.getTime()) ? undefined : data.toISOString();
}

function resumoAlteracoes(anterior: string | null, novo: string | null) {
  if (!anterior && !novo) return '—';
  const texto = (valor: string | null) => {
    if (!valor) return '—';
    try { return JSON.stringify(JSON.parse(valor), null, 2); } catch { return valor; }
  };
  return <details className="max-w-72 text-xs"><summary className="cursor-pointer text-primary hover:underline">Ver alterações</summary><div className="mt-1 space-y-2 whitespace-pre-wrap break-words rounded bg-canvas p-2">{anterior && <p><span className="font-semibold">Antes: </span>{texto(anterior)}</p>}{novo && <p><span className="font-semibold">Depois: </span>{texto(novo)}</p>}</div></details>;
}

export default function HistoricoPage() {
  const [filtros, setFiltros] = React.useState<Filtros>(inicial);
  const consulta = React.useMemo(() => ({
    entidade: filtros.entidade.trim() || undefined, entidadeId: filtros.entidadeId.trim() || undefined,
    acao: filtros.acao.trim() || undefined, de: paraIso(filtros.de), ate: paraIso(filtros.ate),
    pagina: filtros.pagina, tamanhoPagina: filtros.tamanhoPagina,
  }), [filtros]);
  const auditoria = useQuery({ queryKey: queryKeys.auditoria(consulta), queryFn: () => api.get<AuditoriaDtoPaginaDto>('/api/admin/auditoria', consulta) });
  const dados = auditoria.data;
  const alterar = (campo: keyof Pick<Filtros, 'entidade' | 'entidadeId' | 'acao' | 'de' | 'ate'>, valor: string) => setFiltros((atual) => ({ ...atual, [campo]: valor, pagina: 1 }));

  return <>
    <PageHeader title="Histórico de operações" icon={<History />} description="Rastreabilidade das operações registradas no sistema." actions={<Button variant="secondary" size="sm" onClick={() => auditoria.refetch()} loading={auditoria.isFetching}><RefreshCw /> Atualizar</Button>} />
    <Section title="Filtros" description="Refine por ação, entidade, identificador ou período.">
      <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-5">
        <label className="text-xs font-medium text-muted">Ação<Input value={filtros.acao} onChange={(e) => alterar('acao', e.target.value)} placeholder="Ex.: DESPACHAR" /></label>
        <label className="text-xs font-medium text-muted">Entidade<Input value={filtros.entidade} onChange={(e) => alterar('entidade', e.target.value)} placeholder="Ex.: OrdemServico" /></label>
        <label className="text-xs font-medium text-muted">Identificador<Input value={filtros.entidadeId} onChange={(e) => alterar('entidadeId', e.target.value)} placeholder="ID do registro" /></label>
        <label className="text-xs font-medium text-muted">De<Input type="datetime-local" value={filtros.de} onChange={(e) => alterar('de', e.target.value)} /></label>
        <label className="text-xs font-medium text-muted">Até<Input type="datetime-local" value={filtros.ate} onChange={(e) => alterar('ate', e.target.value)} /></label>
      </div>
      <div className="mt-3 flex gap-2"><Button size="sm" variant="secondary" onClick={() => setFiltros(inicial)}><X /> Limpar filtros</Button><span className="inline-flex items-center text-xs text-muted"><Search className="mr-1 h-3.5 w-3.5" /> A busca é aplicada automaticamente.</span></div>
    </Section>
    <div className="mt-3">
      {auditoria.isLoading ? <LoadingState label="Carregando auditoria…" /> : auditoria.isError ? <ErrorState error={auditoria.error} onRetry={() => auditoria.refetch()} title="Não foi possível consultar a auditoria" /> : dados?.itens.length === 0 ? <EmptyState title="Nenhuma operação encontrada" description="Ajuste os filtros ou aguarde novos eventos." /> : <>
        <Table containerClassName="max-h-[calc(100vh-350px)]"><THead><TR><TH>Quando</TH><TH>Usuário</TH><TH>Ação</TH><TH>Entidade</TH><TH>Município</TH><TH>Justificativa</TH><TH>Alterações</TH></TR></THead><TBody>{dados?.itens.map((item) => <TR key={item.id}><TD className="whitespace-nowrap tabular-nums">{formatDateTime(item.ocorridoEm)}</TD><TD>{item.usuario ?? 'Sistema'}</TD><TD><span className="font-mono text-xs">{item.acao ?? '—'}</span></TD><TD><span className="font-medium">{item.entidade ?? '—'}</span>{item.entidadeId && <span className="block max-w-44 truncate font-mono text-xs text-muted" title={item.entidadeId}>{item.entidadeId}</span>}</TD><TD>{item.municipioId ?? '—'}</TD><TD className="max-w-56 whitespace-pre-wrap break-words">{item.justificativa ?? '—'}</TD><TD>{resumoAlteracoes(item.valoresAnteriores, item.valoresNovos)}</TD></TR>)}</TBody></Table>
        <Pagination pagina={dados?.pagina ?? filtros.pagina} tamanhoPagina={dados?.tamanhoPagina ?? filtros.tamanhoPagina} total={dados?.total ?? 0} onPagina={(pagina) => setFiltros((atual) => ({ ...atual, pagina }))} onTamanho={(tamanhoPagina) => setFiltros((atual) => ({ ...atual, tamanhoPagina, pagina: 1 }))} />
      </>}
    </div>
  </>;
}
