import * as React from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { OperacaoHub, type EstadoTempoReal } from '@/services/realtime/operacaoHub';
import { queryKeys } from '@/services/api/queryKeys';

/** Liga o SignalR enquanto houver sessão e invalida as consultas operacionais a cada `filaAlterada`. */
export function useOperacaoRealtime(ativo: boolean, municipioId: number | null): EstadoTempoReal {
  const queryClient = useQueryClient();
  const [estado, setEstado] = React.useState<EstadoTempoReal>('desconectado');
  const hubRef = React.useRef<OperacaoHub | null>(null);
  const municipioRef = React.useRef(municipioId);
  municipioRef.current = municipioId;

  React.useEffect(() => {
    if (!ativo) return;
    const hub = new OperacaoHub(
      () => void queryClient.invalidateQueries({ queryKey: queryKeys.operacao }),
      setEstado,
    );
    hubRef.current = hub;
    void hub.acompanhar(municipioRef.current);
    void hub.iniciar();
    return () => {
      hubRef.current = null;
      void hub.parar();
    };
  }, [ativo, queryClient]);

  React.useEffect(() => {
    void hubRef.current?.acompanhar(municipioId);
  }, [municipioId]);

  return estado;
}
