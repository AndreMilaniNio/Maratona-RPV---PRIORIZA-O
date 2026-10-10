import * as React from 'react';
import { Link } from 'react-router';
import { Marker, Popup } from 'react-leaflet';
import { BaseMap, FitBounds, FlyTo, CENTRO_PADRAO, temCoordenadas } from '@/components/maps/BaseMap';
import { iconeEquipe, iconeOs, iconeSubestacao } from '@/components/maps/markers';
import { PriorityBadge } from '@/features/priorizacao/components/PriorityBadge';
import { ROTAS } from '@/app/config/navigation';
import { formatDateTime } from '@/lib/format';
import { ORIGEM_LOCALIZACAO_EQUIPE_LABEL, STATUS_EQUIPE_LABEL, STATUS_OS_LABEL } from '@/lib/labels';
import type { MapaDto, MapaEquipeDto, MapaOsDto } from '@/types/api';

export interface OperacoesMapProps {
  dados: MapaDto | undefined;
  height?: number | string;
  /** OS em destaque (selecionada na tabela). */
  osSelecionadaId?: string | null;
  onSelecionarOs?: (id: string) => void;
  /** Quando informado, as OS fora deste conjunto ficam atenuadas. */
  osVisiveisIds?: Set<string> | null;
  equipeSelecionadaId?: number | null;
  onSelecionarEquipe?: (id: number) => void;
  mostrarEquipes?: boolean;
  mostrarSubestacoes?: boolean;
  /** Camadas extras (ex.: linha de comparação). */
  children?: React.ReactNode;
  className?: string;
}

/**
 * Mapa operacional: OS por prioridade (críticas em losango), equipes por status
 * (posição demonstrativa sinalizada), subestações. Só desenha o que tem coordenadas.
 */
export function OperacoesMap({
  dados,
  height = 420,
  osSelecionadaId,
  onSelecionarOs,
  osVisiveisIds,
  equipeSelecionadaId,
  onSelecionarEquipe,
  mostrarEquipes = true,
  mostrarSubestacoes = true,
  children,
  className,
}: OperacoesMapProps) {
  const ordens = (dados?.ordens ?? []).filter((o) => temCoordenadas(o.latitude, o.longitude));
  const equipes = (dados?.equipes ?? []).filter((e) => temCoordenadas(e.latitude, e.longitude));
  const subestacoes = (dados?.subestacoes ?? []).filter((s) => temCoordenadas(s.latitude, s.longitude));

  const pontos = React.useMemo<[number, number][]>(
    () => [...ordens.map((o) => [o.latitude, o.longitude] as [number, number]), ...(mostrarEquipes ? equipes.map((e) => [e.latitude, e.longitude] as [number, number]) : [])],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [dados, mostrarEquipes],
  );
  const centro: [number, number] =
    dados?.centroLatitude != null && dados?.centroLongitude != null ? [dados.centroLatitude, dados.centroLongitude] : (pontos[0] ?? CENTRO_PADRAO);
  const selecionada = ordens.find((o) => o.id === osSelecionadaId);

  return (
    <BaseMap center={centro} zoom={13} height={height} className={className} ariaLabel="Mapa das operações">
      <FitBounds points={pontos} />
      <FlyTo point={selecionada ? [selecionada.latitude, selecionada.longitude] : null} />
      {mostrarSubestacoes &&
        subestacoes.map((s) => (
          <Marker key={`se-${s.id}`} position={[s.latitude, s.longitude]} icon={iconeSubestacao(s.codigo)}>
            <Popup>
              <strong>{s.nome}</strong>
              <br />
              Circuito {s.codigo}
              {s.demonstrativo && <div style={{ color: '#7a5a00' }}>Localização demonstrativa</div>}
            </Popup>
          </Marker>
        ))}
      {ordens.map((o) => (
        <OsMarker
          key={o.id}
          os={o}
          selecionada={o.id === osSelecionadaId}
          atenuada={!!osVisiveisIds && !osVisiveisIds.has(o.id)}
          onSelecionar={onSelecionarOs}
        />
      ))}
      {mostrarEquipes &&
        equipes.map((e) => (
          <EquipeMarker key={`eq-${e.id}`} equipe={e} destacada={e.id === equipeSelecionadaId} onSelecionar={onSelecionarEquipe} />
        ))}
      {children}
    </BaseMap>
  );
}

function OsMarker({
  os,
  selecionada,
  atenuada,
  onSelecionar,
}: {
  os: MapaOsDto;
  selecionada: boolean;
  atenuada: boolean;
  onSelecionar?: (id: string) => void;
}) {
  const icon = React.useMemo(
    () => iconeOs({ cor: os.prioridade?.cor, codigo: os.prioridade?.codigo, critica: os.critica, selecionada, atenuada }),
    [os.prioridade?.cor, os.prioridade?.codigo, os.critica, selecionada, atenuada],
  );
  return (
    <Marker
      position={[os.latitude, os.longitude]}
      icon={icon}
      zIndexOffset={selecionada ? 1000 : os.critica ? 500 : 0}
      eventHandlers={onSelecionar ? { click: () => onSelecionar(os.id) } : undefined}
      title={`OS ${os.numero ?? ''}`}
    >
      <Popup>
        <div style={{ minWidth: 200 }}>
          <Link to={ROTAS.os(os.id)} style={{ fontWeight: 700 }}>
            {os.numero}
          </Link>
          <div style={{ margin: '4px 0' }}>
            <PriorityBadge prioridade={os.prioridade} />
          </div>
          <div>{os.tipoOcorrencia}</div>
          <div>{STATUS_OS_LABEL[os.status] ?? os.status}</div>
          <div style={{ color: '#5b6b80' }}>{os.endereco}</div>
        </div>
      </Popup>
    </Marker>
  );
}

function EquipeMarker({ equipe, destacada, onSelecionar }: { equipe: MapaEquipeDto; destacada: boolean; onSelecionar?: (id: number) => void }) {
  const demonstrativa = equipe.origem === 'Demonstrativa';
  const icon = React.useMemo(
    () => iconeEquipe({ status: equipe.status, codigo: equipe.codigo, destacada, demonstrativa }),
    [equipe.status, equipe.codigo, destacada, demonstrativa],
  );
  return (
    <Marker
      position={[equipe.latitude, equipe.longitude]}
      icon={icon}
      zIndexOffset={destacada ? 900 : 100}
      eventHandlers={onSelecionar ? { click: () => onSelecionar(equipe.id) } : undefined}
      title={`Equipe ${equipe.codigo ?? ''}`}
    >
      <Popup>
        <div style={{ minWidth: 190 }}>
          <strong>
            {equipe.codigo} — {equipe.nome}
          </strong>
          <div>{STATUS_EQUIPE_LABEL[equipe.status] ?? equipe.status}</div>
          <div style={{ color: demonstrativa ? '#7a5a00' : '#5b6b80' }}>
            {ORIGEM_LOCALIZACAO_EQUIPE_LABEL[equipe.origem] ?? equipe.origem}
            {equipe.localizacaoEm ? ` · ${formatDateTime(equipe.localizacaoEm)}` : ''}
          </div>
        </div>
      </Popup>
    </Marker>
  );
}
