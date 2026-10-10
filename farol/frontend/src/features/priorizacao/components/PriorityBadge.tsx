import { CircleArrowDown, CircleDot, CircleHelp, OctagonAlert, TriangleAlert, UserCog, type LucideIcon } from 'lucide-react';
import type { PrioridadeResumoDto } from '@/types/api';
import { cn, corTextoSobre } from '@/lib/utils';

/** Ícone por nível: nunca dependemos só da cor para comunicar criticidade. */
export function iconePrioridade(p: Pick<PrioridadeResumoDto, 'rank' | 'critica'> | null | undefined): LucideIcon {
  if (!p) return CircleHelp;
  if (p.critica && p.rank <= 1) return OctagonAlert;
  if (p.critica || p.rank <= 2) return TriangleAlert;
  if (p.rank <= 4) return CircleDot;
  return CircleArrowDown;
}

/**
 * Selo de prioridade: cor (vinda da API, `prioridade.cor`) + texto (código e nome) + ícone.
 */
export function PriorityBadge({
  prioridade,
  manual,
  compact,
  className,
}: {
  prioridade: PrioridadeResumoDto | null | undefined;
  /** Prioridade definida manualmente (reclassificação com justificativa). */
  manual?: boolean;
  /** Mostra só o código (tabelas estreitas); o nome fica no title/aria-label. */
  compact?: boolean;
  className?: string;
}) {
  if (!prioridade) {
    return (
      <span className={cn('inline-flex items-center gap-1 rounded border border-line px-1.5 py-0.5 text-xs text-muted', className)}>
        <CircleHelp className="h-3.5 w-3.5" aria-hidden />
        Sem prioridade
      </span>
    );
  }
  const Icone = iconePrioridade(prioridade);
  const cor = prioridade.cor ?? '#5b6b80';
  const descricao = `Prioridade ${prioridade.nome ?? ''} (${prioridade.codigo ?? ''})${prioridade.critica ? ', crítica' : ''}${manual ? ', definida manualmente' : ''}`;
  return (
    <span
      className={cn('inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-xs font-semibold leading-none whitespace-nowrap', className)}
      style={{ backgroundColor: cor, color: corTextoSobre(cor) }}
      title={descricao}
      aria-label={descricao}
      data-testid="priority-badge"
      data-cor={cor}
    >
      <Icone className="h-3.5 w-3.5 shrink-0" aria-hidden data-testid="priority-icon" />
      <span>{prioridade.codigo}</span>
      {!compact && <span className="font-medium">· {prioridade.nome}</span>}
      {manual && <UserCog className="h-3.5 w-3.5 shrink-0" aria-hidden />}
    </span>
  );
}
