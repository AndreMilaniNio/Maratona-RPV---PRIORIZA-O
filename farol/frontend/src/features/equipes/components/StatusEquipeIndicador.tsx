import { CheckCircle2, MinusCircle } from 'lucide-react';
import { COR_STATUS_EQUIPE } from '@/components/maps/markers';
import { STATUS_EQUIPE_LABEL } from '@/lib/labels';
import { cn } from '@/lib/utils';
import type { EquipeDto, StatusEquipe } from '@/types/api';
import { motivoIndisponibilidade } from '@/features/equipes/utils/equipeExibicao';

/** Status operacional da equipe: ponto colorido + texto (a cor nunca aparece sozinha). */
export function StatusEquipeIndicador({ status, className }: { status: StatusEquipe; className?: string }) {
  const cor = COR_STATUS_EQUIPE[status] ?? '#6b7280';
  return (
    <span className={cn('inline-flex items-center gap-1.5 whitespace-nowrap', className)} data-testid="status-equipe">
      <span aria-hidden className="inline-block size-2.5 shrink-0 rounded-full" style={{ backgroundColor: cor }} data-testid="status-equipe-cor" />
      <span>{STATUS_EQUIPE_LABEL[status] ?? status}</span>
    </span>
  );
}

/** Disponível para despacho (vem da API) e, quando não, o motivo + ocupação da capacidade. */
export function DisponibilidadeEquipe({
  equipe,
  className,
}: {
  equipe: Pick<EquipeDto, 'disponivel' | 'ativa' | 'status' | 'despachosAtivos' | 'capacidade'>;
  className?: string;
}) {
  const motivo = motivoIndisponibilidade(equipe);
  const ocupacao = `${equipe.despachosAtivos}/${equipe.capacidade} despacho(s)`;
  if (equipe.disponivel) {
    return (
      <span className={cn('inline-flex flex-col', className)}>
        <span className="inline-flex items-center gap-1 font-medium text-success">
          <CheckCircle2 className="size-3.5" aria-hidden /> Disponível
        </span>
        <span className="text-xs text-muted">{ocupacao}</span>
      </span>
    );
  }
  return (
    <span className={cn('inline-flex flex-col', className)}>
      <span className="inline-flex items-center gap-1 font-medium text-[#5b6b80]">
        <MinusCircle className="size-3.5" aria-hidden /> Indisponível
      </span>
      <span className="text-xs text-muted">
        {motivo}
        {motivo && !motivo.startsWith('Capacidade') ? ` · ${ocupacao}` : ''}
      </span>
    </span>
  );
}
