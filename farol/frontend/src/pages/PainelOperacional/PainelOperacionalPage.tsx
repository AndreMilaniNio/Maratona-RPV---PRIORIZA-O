import { useNavigate } from 'react-router';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { LoadingState, ErrorState, EmptyState } from '@/components/feedback/states';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { useCatalogo } from '@/hooks/useCatalogo';
import { useFila, useIndicadores } from '@/features/ordens-servico/hooks/useFila';
import { useFiltroPainel, useOsAbertaNaUrl } from '@/features/ordens-servico/hooks/useFiltroPainel';
import { IndicatorsRow } from '@/features/ordens-servico/components/painel/IndicatorsRow';
import { QueueFilters } from '@/features/ordens-servico/components/painel/QueueFilters';
import { QueueTable } from '@/features/ordens-servico/components/painel/QueueTable';
import { COLUNAS } from '@/features/ordens-servico/components/painel/colunas';
import { filtroParaQuery } from '@/features/ordens-servico/schemas/filtroFila';
import { ROTAS } from '@/app/config/navigation';

export default function PainelOperacionalPage() {
  const { municipioId, municipio } = useCidade(); const nav = useNavigate(); const { filtro, alterar, limpar } = useFiltroPainel(); const { osId, abrir } = useOsAbertaNaUrl();
  const fila = useFila(filtroParaQuery(filtro, municipioId)); const indicadores = useIndicadores(municipioId); const catalogo = useCatalogo();
  const selecionarIndicador = (chave: string) => alterar(chave === 'aguardandoDespacho' ? { status: ['AguardandoDespacho'] } : chave === 'emAtendimento' ? { status: ['EmExecucao'] } : chave === 'vencidas' ? { prazo: 'vencido' } : {});
  return <div className="space-y-3"><PageHeader title="Painel operacional" description={`Fila priorizada de ${municipio?.nome ?? 'todas as cidades'}.`} />
    <IndicatorsRow dados={indicadores.data} carregando={indicadores.isLoading} erro={indicadores.error} onRetry={() => void indicadores.refetch()} mostrarPorCidade={municipioId === null} onSelecionar={selecionarIndicador} />
    <QueueFilters filtro={filtro} municipioId={municipioId} onAlterar={alterar} onLimpar={limpar} />
    <Section title="Fila de atendimento" description={fila.data ? `${fila.data.total} ordem(ns) encontrada(s).` : undefined}>{fila.isLoading ? <LoadingState label="Carregando fila..." /> : fila.error ? <ErrorState error={fila.error} onRetry={() => void fila.refetch()} /> : !fila.data?.itens.length ? <EmptyState title="Nenhuma OS neste filtro" /> : <QueueTable itens={fila.data.itens} colunas={COLUNAS} ordenarPor={filtro.ordenarPor} onOrdenar={(ordenarPor) => alterar({ ordenarPor })} agora={new Date()} selecionadaId={osId} podeDespachar={true} catalogo={catalogo.data} onSelecionar={abrir} onAbrir={(id) => nav(ROTAS.os(id))} onDespachar={(id) => nav(ROTAS.os(id))} atualizando={fila.isFetching} />}</Section>
  </div>;
}
