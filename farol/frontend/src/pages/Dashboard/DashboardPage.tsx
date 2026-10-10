import { Link } from 'react-router';
import { ArrowRight, FilePlus2, ListChecks, Map } from 'lucide-react';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { LoadingState, ErrorState, EmptyState } from '@/components/feedback/states';
import { Button } from '@/components/ui/button';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { PriorityBadge } from '@/features/priorizacao/components/PriorityBadge';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { useFila, useIndicadores } from '@/features/ordens-servico/hooks/useFila';
import { FILTRO_PADRAO, filtroParaQuery } from '@/features/ordens-servico/schemas/filtroFila';
import { IndicatorsRow } from '@/features/ordens-servico/components/painel/IndicatorsRow';
import { ROTAS } from '@/app/config/navigation';

export default function DashboardPage() {
  const { municipioId, municipio } = useCidade(); const indicadores = useIndicadores(municipioId); const fila = useFila(filtroParaQuery({ ...FILTRO_PADRAO, tamanhoPagina: 6 }, municipioId));
  return <div className="space-y-3"><PageHeader title="Dashboard operacional" description={`Visão atual de ${municipio?.nome ?? 'todas as cidades'}.`} actions={<><Link to={ROTAS.novaSolicitacao}><Button><FilePlus2 /> Nova solicitação</Button></Link><Link to={ROTAS.painel}><Button variant="secondary"><ListChecks /> Abrir fila</Button></Link></>} />
    <IndicatorsRow dados={indicadores.data} carregando={indicadores.isLoading} erro={indicadores.error} onRetry={() => void indicadores.refetch()} mostrarPorCidade={municipioId === null} />
    <Section title="Ordens no topo da fila" description="Atualização automática pelo servidor." actions={<Link to={ROTAS.painel} className="inline-flex items-center gap-1 text-sm text-primary hover:underline">Ver fila completa <ArrowRight className="size-4" /></Link>}>
      {fila.isLoading ? <LoadingState /> : fila.error ? <ErrorState error={fila.error} onRetry={() => void fila.refetch()} /> : fila.data?.itens.length ? <Table><THead><TR><TH>Pos.</TH><TH>OS</TH><TH>Prioridade</TH><TH>Ocorrência</TH><TH>Status</TH></TR></THead><TBody>{fila.data.itens.map((o) => <TR key={o.id}><TD>{o.posicao ?? '—'}</TD><TD><Link className="font-medium text-primary hover:underline" to={ROTAS.os(o.id)}>{o.numero}</Link></TD><TD><PriorityBadge prioridade={o.prioridade} /></TD><TD>{o.tipoOcorrencia}</TD><TD>{o.status}</TD></TR>)}</TBody></Table> : <EmptyState title="Nenhuma OS aberta" description="Registre uma solicitação para iniciar a operação." />}
    </Section><Section title="Acompanhamento geográfico" description="Veja ordens, equipes e subestações no mapa operacional."><Link to={ROTAS.mapa}><Button variant="secondary"><Map /> Abrir mapa</Button></Link></Section></div>;
}
