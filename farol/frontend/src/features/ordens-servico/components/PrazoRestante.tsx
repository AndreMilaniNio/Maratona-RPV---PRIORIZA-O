import * as React from 'react';
import { AlarmClock, Clock, TimerOff } from 'lucide-react';
import type { ProximoPrazoDto } from '@/types/api';
import { formatDateTime, formatPrazoRestante } from '@/lib/format';
import { TIPO_PRAZO_LABEL } from '@/lib/labels';
import { cn } from '@/lib/utils';

/** Relógio compartilhado que re-renderiza a cada 30 s (tempo restante "ao vivo"). */
export function useAgora(intervaloMs = 30_000): Date {
  const [agora, setAgora] = React.useState(() => new Date());
  React.useEffect(() => {
    const t = setInterval(() => setAgora(new Date()), intervaloMs);
    return () => clearInterval(t);
  }, [intervaloMs]);
  return agora;
}

/** Tempo restante / atraso do próximo prazo, com texto + ícone (não só cor). */
export function PrazoRestante({
  prazo,
  agora,
  mostrarTipo,
  className,
}: {
  prazo: ProximoPrazoDto | null | undefined;
  agora?: Date;
  mostrarTipo?: boolean;
  className?: string;
}) {
  const f = formatPrazoRestante(prazo, agora);
  const Icone = f.estado === 'vencido' ? TimerOff : f.estado === 'proximo' ? AlarmClock : Clock;
  const cls = {
    vencido: 'text-danger font-semibold',
    proximo: 'text-warning font-semibold',
    ok: 'text-ink',
    'sem-prazo': 'text-muted',
  }[f.estado];
  const titulo = prazo?.limite ? `Prazo de ${TIPO_PRAZO_LABEL[prazo.tipo] ?? prazo.tipo}: ${formatDateTime(prazo.limite)}` : undefined;
  return (
    <span className={cn('inline-flex items-center gap-1 whitespace-nowrap', cls, className)} title={titulo} data-estado={f.estado}>
      <Icone className="h-3.5 w-3.5 shrink-0" aria-hidden />
      <span>
        {f.texto}
        {mostrarTipo && prazo && <span className="font-normal text-muted"> ({TIPO_PRAZO_LABEL[prazo.tipo] ?? prazo.tipo})</span>}
      </span>
    </span>
  );
}
