import * as React from 'react';
import { useNavigate } from 'react-router';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { LoadingState, ErrorState, InlineAlert } from '@/components/feedback/states';
import { OperacoesMap } from '@/components/maps/OperacoesMap';
import { CheckboxField } from '@/components/ui/checkbox';
import { NativeSelect } from '@/components/ui/input';
import { useMapaOperacoes } from '@/features/ordens-servico/services/mapaService';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { ROTAS } from '@/app/config/navigation';

export default function MapaOperacionalPage() {
  const { municipioId, municipio } = useCidade();
  const mapa = useMapaOperacoes(municipioId);
  const nav = useNavigate();
  const [selecionada, setSelecionada] = React.useState<string | null>(null);
  const [prioridade, setPrioridade] = React.useState('todas');
  const [circuito, setCircuito] = React.useState('todos');
  const [osId, setOsId] = React.useState('todas');
  const [mostrarCircuitos, setMostrarCircuitos] = React.useState(true);
  const [mostrarPostes, setMostrarPostes] = React.useState(true);

  const ordens = mapa.data?.ordens ?? [];
  const prioridades = React.useMemo(() => [...new Map(ordens.filter((o) => o.prioridade?.codigo).map((o) => [o.prioridade!.codigo!, o.prioridade!])).values()], [ordens]);
  const circuitos = React.useMemo(() => [...new Map(ordens.filter((o) => o.circuitoCodigo).map((o) => [o.circuitoCodigo!, o.circuitoNome ?? o.circuitoCodigo!])).entries()], [ordens]);
  const filtroAtivo = prioridade !== 'todas' || circuito !== 'todos' || osId !== 'todas';
  const dadosFiltrados = React.useMemo(() => {
    if (!mapa.data) return mapa.data;
    return {
      ...mapa.data,
      ordens: ordens.filter((o) => (prioridade === 'todas' || o.prioridade?.codigo === prioridade) && (circuito === 'todos' || o.circuitoCodigo === circuito) && (osId === 'todas' || o.id === osId) && (mostrarPostes || !/poste/i.test(o.tipoOcorrencia ?? ''))),
    };
  }, [mapa.data, ordens, prioridade, circuito, osId, mostrarPostes]);

  if (mapa.isLoading) return <LoadingState label="Carregando mapa operacional..." />; if (mapa.error) return <ErrorState error={mapa.error} onRetry={() => void mapa.refetch()} />;
  return <div className="space-y-3">
    <PageHeader title="Mapa operacional" description={`Ordens e equipes de ${municipio?.nome ?? 'todas as cidades'}.`} />
    <Section title="Operação em campo" description={`${dadosFiltrados?.ordens.length ?? 0} OS exibida(s). Passe o mouse sobre um marcador para ver o resumo.`}>
      <div className="mx-auto mb-3 flex w-full max-w-5xl flex-wrap justify-center gap-3 rounded-md bg-canvas p-3">
        <label className="grid w-full gap-1 text-xs font-medium text-muted md:w-[calc(33.333%-0.75rem)]"><span>Prioridade</span><NativeSelect value={prioridade} onChange={(e) => setPrioridade(e.target.value)}><option value="todas">Todas as prioridades</option>{prioridades.map((p) => <option key={p.codigo!} value={p.codigo!}>{p.nome} ({p.codigo})</option>)}</NativeSelect></label>
        <label className="grid w-full gap-1 text-xs font-medium text-muted md:w-[calc(33.333%-0.75rem)]"><span>Circuito</span><NativeSelect value={circuito} onChange={(e) => setCircuito(e.target.value)}><option value="todos">Todos os circuitos</option>{circuitos.map(([codigo, nome]) => <option key={codigo} value={codigo}>{codigo} — {nome}</option>)}</NativeSelect></label>
        <label className="grid w-full gap-1 text-xs font-medium text-muted md:w-[calc(33.333%-0.75rem)]"><span>Ordem de serviço</span><NativeSelect value={osId} onChange={(e) => setOsId(e.target.value)}><option value="todas">Todas as OS</option>{ordens.map((o) => <option key={o.id} value={o.id}>{o.numero ?? o.id} — {o.tipoOcorrencia ?? 'Ocorrência'}</option>)}</NativeSelect></label>
        <div className="flex w-full flex-wrap items-center justify-center gap-x-5 gap-y-2 border-t border-line pt-2 text-left">
          <CheckboxField id="mapa-circuitos" label="Exibir circuitos" checked={mostrarCircuitos} onCheckedChange={setMostrarCircuitos} />
          <CheckboxField id="mapa-postes" label="Exibir OS de poste" checked={mostrarPostes} onCheckedChange={setMostrarPostes} />
        </div>
      </div>
      <OperacoesMap className="mx-auto w-full" dados={dadosFiltrados} height="calc(100vh - 360px)" mostrarEquipes={!filtroAtivo} mostrarSubestacoes={mostrarCircuitos && !filtroAtivo} osSelecionadaId={selecionada} onSelecionarOs={(id) => { setSelecionada(id); nav(ROTAS.os(id)); }} />
    </Section>
    {(mapa.data?.semCoordenadas ?? 0) > 0 && <InlineAlert tone="warning" title="OS sem coordenadas"><p>{mapa.data?.semCoordenadas} ordem(ns) não podem ser exibidas no mapa até a localização ser confirmada.</p></InlineAlert>}
  </div>;
}
