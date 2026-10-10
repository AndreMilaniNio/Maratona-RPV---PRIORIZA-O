import * as React from 'react';
import { MapContainer, TileLayer, useMap } from 'react-leaflet';
import L, { type LatLngBoundsExpression, type LatLngExpression } from 'leaflet';
import { TILE_ATTRIBUTION, TILE_URL } from '@/app/config/env';
import { cn } from '@/lib/utils';

/** Centro padrão (região dos municípios demonstrativos) quando não há pontos. */
export const CENTRO_PADRAO: [number, number] = [-21.527, -42.637];

/**
 * Mapa base Leaflet + OpenStreetMap. Os filhos são camadas react-leaflet.
 * `height` define a altura do contêiner (o Leaflet exige altura explícita).
 */
export function BaseMap({
  center = CENTRO_PADRAO,
  zoom = 13,
  height = 360,
  className,
  children,
  scrollWheelZoom = true,
  ariaLabel = 'Mapa',
}: {
  center?: LatLngExpression;
  zoom?: number;
  height?: number | string;
  className?: string;
  children?: React.ReactNode;
  scrollWheelZoom?: boolean;
  ariaLabel?: string;
}) {
  return (
    <div className={cn('relative overflow-hidden rounded-md border border-line', className)} style={{ height }} role="region" aria-label={ariaLabel}>
      <MapContainer center={center} zoom={zoom} scrollWheelZoom={scrollWheelZoom} style={{ height: '100%', width: '100%' }}>
        <TileLayer url={TILE_URL} attribution={TILE_ATTRIBUTION} />
        <InvalidateOnResize />
        {children}
      </MapContainer>
    </div>
  );
}

/** Corrige o tamanho do mapa quando o contêiner muda (painel recolhível, modal). */
function InvalidateOnResize() {
  const map = useMap();
  React.useEffect(() => {
    const el = map.getContainer();
    const t = setTimeout(() => map.invalidateSize(), 50);
    if (typeof ResizeObserver === 'undefined') return () => clearTimeout(t);
    const ro = new ResizeObserver(() => map.invalidateSize());
    ro.observe(el);
    return () => {
      clearTimeout(t);
      ro.disconnect();
    };
  }, [map]);
  return null;
}

/** Ajusta o enquadramento aos pontos informados (uma vez por mudança da chave). */
export function FitBounds({ points, maxZoom = 15, padding = 30 }: { points: [number, number][]; maxZoom?: number; padding?: number }) {
  const map = useMap();
  const key = points.map((p) => p.join(',')).join(';');
  React.useEffect(() => {
    if (points.length === 0) return;
    if (points.length === 1) {
      map.setView(points[0], Math.min(maxZoom, 16));
      return;
    }
    const bounds = L.latLngBounds(points as LatLngBoundsExpression as L.LatLngTuple[]);
    map.fitBounds(bounds, { padding: [padding, padding], maxZoom });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key]);
  return null;
}

/** Centraliza o mapa num ponto quando ele muda (ex.: OS selecionada na tabela). */
export function FlyTo({ point, zoom = 16 }: { point: [number, number] | null; zoom?: number }) {
  const map = useMap();
  const key = point ? point.join(',') : '';
  React.useEffect(() => {
    if (point) map.flyTo(point, Math.max(map.getZoom(), zoom), { duration: 0.4 });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key]);
  return null;
}

export function temCoordenadas(lat: number | null | undefined, lng: number | null | undefined): boolean {
  return typeof lat === 'number' && typeof lng === 'number' && Number.isFinite(lat) && Number.isFinite(lng);
}
