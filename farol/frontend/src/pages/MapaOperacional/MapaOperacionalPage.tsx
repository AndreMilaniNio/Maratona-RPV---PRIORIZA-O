import * as React from 'react';
import { useNavigate } from 'react-router';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { LoadingState, ErrorState, InlineAlert } from '@/components/feedback/states';
import { OperacoesMap } from '@/components/maps/OperacoesMap';
import { useMapaOperacoes } from '@/features/ordens-servico/services/mapaService';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { ROTAS } from '@/app/config/navigation';

export default function MapaOperacionalPage() { const { municipioId, municipio } = useCidade(); const mapa = useMapaOperacoes(municipioId); const nav = useNavigate(); const [selecionada, setSelecionada] = React.useState<string | null>(null);
  if (mapa.isLoading) return <LoadingState label="Carregando mapa operacional..." />; if (mapa.error) return <ErrorState error={mapa.error} onRetry={() => void mapa.refetch()} />;
  return <div className="space-y-3"><PageHeader title="Mapa operacional" description={`Ordens e equipes de ${municipio?.nome ?? 'todas as cidades'}.`} /><Section title="Operação em campo"><OperacoesMap dados={mapa.data} height="calc(100vh - 250px)" osSelecionadaId={selecionada} onSelecionarOs={(id) => { setSelecionada(id); nav(ROTAS.os(id)); }} /></Section>{(mapa.data?.semCoordenadas ?? 0) > 0 && <InlineAlert tone="warning" title="OS sem coordenadas"><p>{mapa.data?.semCoordenadas} ordem(ns) não podem ser exibidas no mapa até a localização ser confirmada.</p></InlineAlert>}</div>; }
