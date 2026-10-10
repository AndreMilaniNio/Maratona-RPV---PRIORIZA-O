import * as React from 'react';
import { Marker, Popup } from 'react-leaflet';
import { BaseMap, FitBounds, temCoordenadas } from '@/components/maps/BaseMap';
import { iconeEquipe, iconeOs } from '@/components/maps/markers';
import { InlineAlert } from '@/components/feedback/states';
import { useRota } from '@/features/ordens-servico/services/mapaService';
import { formatDateTime, formatDistanciaKm } from '@/lib/format';
import { STATUS_EQUIPE_LABEL, label } from '@/lib/labels';
import type { CandidatasDto, EquipeCandidataDto, PrioridadeResumoDto } from '@/types/api';
import { LinhaComparacao, ResumoRota } from '@/features/despacho/components/RotaComparacao';

type Ponto = [number, number];

/** Mapa do despacho: destino da OS + equipes candidatas; a equipe escolhida ganha a linha de comparação. */
export function CandidatasMapa({
  dados,
  candidatas,
  selecionadaId,
  onSelecionar,
  prioridade,
  height = 420,
}: {
  dados: CandidatasDto;
  candidatas: EquipeCandidataDto[];
  selecionadaId: number | null;
  onSelecionar: (id: number) => void;
  prioridade?: PrioridadeResumoDto | null;
  height?: number | string;
}) {
  const destino: Ponto | null = temCoordenadas(dados.osLatitude, dados.osLongitude) ? [dados.osLatitude!, dados.osLongitude!] : null;
  const comPosicao = candidatas.filter((c) => c.equipe && temCoordenadas(c.equipe.latitude, c.equipe.longitude));
  const selecionada = comPosicao.find((c) => c.equipe!.id === selecionadaId) ?? null;
  const origem: Ponto | null = selecionada ? [selecionada.equipe!.latitude!, selecionada.equipe!.longitude!] : null;
  const rota = useRota(origem, destino);

  const pontos = React.useMemo<Ponto[]>(
    () => [...(destino ? [destino] : []), ...comPosicao.map((c) => [c.equipe!.latitude!, c.equipe!.longitude!] as Ponto)],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dados.osId, candidatas],
  );
  const iconeDestino = React.useMemo(
    () => iconeOs({ cor: prioridade?.cor, codigo: prioridade?.codigo ?? 'OS', critica: prioridade?.critica, selecionada: true }),
    [prioridade?.cor, prioridade?.codigo, prioridade?.critica],
  );

  return (
    <div className="space-y-2">
      {!destino && (
        <InlineAlert tone="warning" title="OS sem coordenadas">
          A localização é aproximada ou depende de confirmação; distâncias não podem ser calculadas.
        </InlineAlert>
      )}
      <BaseMap height={height} ariaLabel="Mapa do despacho: destino da OS e equipes candidatas">
        <FitBounds points={pontos} />
        {destino && (
          <Marker position={destino} icon={iconeDestino} zIndexOffset={1000} title={`Destino: OS ${dados.osNumero ?? ''}`}>
            <Popup>
              <strong>Destino — OS {dados.osNumero}</strong>
            </Popup>
          </Marker>
        )}
        {comPosicao.map((c) => (
          <MarcadorCandidata key={c.equipe!.id} candidata={c} selecionada={c.equipe!.id === selecionadaId} onSelecionar={onSelecionar} />
        ))}
        {origem && destino && <LinhaComparacao de={origem} para={destino} rota={rota.data} />}
      </BaseMap>
      {selecionada && destino ? (
        <ResumoRota rota={rota.data} carregando={rota.isFetching} erro={rota.error} titulo={`${selecionada.equipe!.codigo} × OS ${dados.osNumero ?? ''}`} />
      ) : (
        <p className="text-xs text-muted">Selecione uma equipe para ver a linha de comparação até a ocorrência.</p>
      )}
    </div>
  );
}

function MarcadorCandidata({
  candidata,
  selecionada,
  onSelecionar,
}: {
  candidata: EquipeCandidataDto;
  selecionada: boolean;
  onSelecionar: (id: number) => void;
}) {
  const e = candidata.equipe!;
  const demonstrativa = e.origemLocalizacao === 'Demonstrativa';
  const icon = React.useMemo(
    () => iconeEquipe({ status: e.status, codigo: e.codigo, destacada: selecionada, recomendada: candidata.recomendada, demonstrativa }),
    [e.status, e.codigo, selecionada, candidata.recomendada, demonstrativa],
  );
  return (
    <Marker
      position={[e.latitude!, e.longitude!]}
      icon={icon}
      zIndexOffset={selecionada ? 900 : candidata.recomendada ? 800 : 100}
      eventHandlers={{ click: () => onSelecionar(e.id) }}
      title={`Equipe ${e.codigo ?? ''}`}
    >
      <Popup>
        <strong>
          {e.codigo} — {e.nome}
        </strong>
        {candidata.recomendada && <div style={{ color: '#667A45', fontWeight: 600 }}>Recomendada</div>}
        <div>{label(STATUS_EQUIPE_LABEL, e.status)}</div>
        <div>{formatDistanciaKm(candidata.distanciaKm)}</div>
        <div style={{ color: demonstrativa ? '#7a5a00' : '#5b6b80' }}>
          {demonstrativa ? 'Posição demonstrativa' : 'Posição conhecida'}
          {e.localizacaoEm ? ` · ${formatDateTime(e.localizacaoEm)}` : ''}
        </div>
      </Popup>
    </Marker>
  );
}
