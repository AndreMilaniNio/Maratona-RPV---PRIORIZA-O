import { Polyline, Tooltip as LeafletTooltip } from 'react-leaflet';
import { Route } from 'lucide-react';
import { Spinner } from '@/components/ui/misc';
import { formatDistanciaKm, formatDuration } from '@/lib/format';
import { mensagemErro } from '@/services/api/client';
import { cn } from '@/lib/utils';
import type { RotaDto } from '@/types/api';

type Ponto = [number, number];

/** A rota é "real" só quando o serviço de roteamento respondeu com tempo. */
export function rotaTemTempo(rota: RotaDto | undefined | null): boolean {
  return !!rota && rota.disponivel && rota.tempoMin != null;
}

/** Camada do mapa: geometria da rota quando houver; senão, linha reta tracejada. */
export function LinhaComparacao({ de, para, rota }: { de: Ponto; para: Ponto; rota: RotaDto | undefined | null }) {
  const geometria = rota?.geometria && rota.geometria.length >= 2 ? (rota.geometria.map((p) => [p[0], p[1]]) as Ponto[]) : [de, para];
  const comTempo = rotaTemTempo(rota);
  return (
    <Polyline
      positions={geometria}
      pathOptions={{ color: comTempo ? '#2457A6' : '#17365D', weight: comTempo ? 4 : 3, dashArray: comTempo ? undefined : '6 6', opacity: 0.9 }}
    >
      <LeafletTooltip sticky>{textoRota(rota)}</LeafletTooltip>
    </Polyline>
  );
}

export function textoRota(rota: RotaDto | undefined | null): string {
  if (!rota) return 'Calculando distância…';
  if (rotaTemTempo(rota)) return `${formatDistanciaKm(rota.distanciaKm)} · ${formatDuration(rota.tempoMin!)} (rota estimada)`;
  return `${formatDistanciaKm(rota.distanciaKm)} — distância em linha reta — sem estimativa de tempo`;
}

/** Resumo textual da comparação equipe × ocorrência (distância, tempo, aviso). */
export function ResumoRota({
  rota,
  carregando,
  erro,
  titulo = 'Comparação equipe × ocorrência',
  className,
}: {
  rota: RotaDto | undefined | null;
  carregando?: boolean;
  erro?: unknown;
  titulo?: string;
  className?: string;
}) {
  return (
    <div className={cn('rounded-md border border-line bg-white px-3 py-2 text-[13px]', className)} aria-live="polite" data-testid="resumo-rota">
      <p className="flex items-center gap-1 font-medium text-navy">
        <Route className="h-4 w-4" aria-hidden /> {titulo}
      </p>
      {carregando && !rota ? (
        <p className="flex items-center gap-2 text-muted">
          <Spinner /> Calculando…
        </p>
      ) : erro && !rota ? (
        <p className="text-danger">Não foi possível calcular a distância: {mensagemErro(erro)}</p>
      ) : rota ? (
        <>
          <p>
            Distância: <strong>{formatDistanciaKm(rota.distanciaKm)}</strong>
            {' · '}
            {rotaTemTempo(rota) ? (
              <>
                Tempo estimado: <strong>{formatDuration(rota.tempoMin!)}</strong>
              </>
            ) : (
              <span className="font-medium text-[#7a3306]">distância em linha reta — sem estimativa de tempo</span>
            )}
          </p>
          {rota.aviso && <p className="text-xs text-muted">{rota.aviso}</p>}
        </>
      ) : null}
    </div>
  );
}
