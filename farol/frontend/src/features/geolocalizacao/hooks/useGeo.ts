import * as React from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { queryKeys } from '@/services/api/queryKeys';
import { apenasDigitos } from '@/lib/utils';
import { geoService } from '../services/geoService';

/** Consulta o CEP quando ele tem 8 dígitos. */
export function useCep(cep: string) {
  const digitos = apenasDigitos(cep);
  return useQuery({
    queryKey: queryKeys.cep(digitos),
    queryFn: ({ signal }) => geoService.cep(digitos, signal),
    enabled: digitos.length === 8,
    retry: false,
    staleTime: 30 * 60_000,
  });
}

/** Interpretação de coordenadas coladas (texto, link de mapa ou GMS). */
export function useInterpretarCoordenadas() {
  return useMutation({ mutationFn: geoService.interpretarCoordenadas });
}

export type EstadoGeolocalizacao =
  | { estado: 'ocioso' }
  | { estado: 'obtendo' }
  | { estado: 'erro'; mensagem: string };

/** Geolocalização do dispositivo (navigator.geolocation), com mensagens em português. */
export function useGeolocalizacaoDispositivo() {
  const [status, setStatus] = React.useState<EstadoGeolocalizacao>({ estado: 'ocioso' });
  const disponivel = typeof navigator !== 'undefined' && 'geolocation' in navigator;

  const obter = React.useCallback(
    (onSucesso: (p: { latitude: number; longitude: number; precisao: number | null }) => void) => {
      if (!disponivel) {
        setStatus({ estado: 'erro', mensagem: 'Este navegador não oferece geolocalização.' });
        return;
      }
      setStatus({ estado: 'obtendo' });
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          setStatus({ estado: 'ocioso' });
          onSucesso({
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
            precisao: Number.isFinite(pos.coords.accuracy) ? Math.round(pos.coords.accuracy) : null,
          });
        },
        (err) => {
          const mensagem =
            err.code === err.PERMISSION_DENIED
              ? 'Permissão de localização negada no navegador.'
              : err.code === err.TIMEOUT
                ? 'Tempo esgotado ao obter a localização.'
                : 'Não foi possível obter a localização do dispositivo.';
          setStatus({ estado: 'erro', mensagem });
        },
        { enableHighAccuracy: true, timeout: 15_000, maximumAge: 0 },
      );
    },
    [disponivel],
  );

  return { status, obter, disponivel };
}
