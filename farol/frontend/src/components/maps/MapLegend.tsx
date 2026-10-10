import { COR_STATUS_EQUIPE } from '@/components/maps/markers';
import { STATUS_EQUIPE_LABEL } from '@/lib/labels';
import type { PrioridadeResumoDto, StatusEquipe } from '@/types/api';
import { cn } from '@/lib/utils';

/** Legenda dos marcadores do mapa (prioridades vindas da API + status de equipe + subestação). */
export function MapLegend({
  prioridades,
  semCoordenadas,
  className,
  mostrarEquipes = true,
  mostrarSubestacoes = true,
}: {
  prioridades: Pick<PrioridadeResumoDto, 'codigo' | 'nome' | 'cor' | 'critica'>[];
  semCoordenadas?: number;
  className?: string;
  mostrarEquipes?: boolean;
  mostrarSubestacoes?: boolean;
}) {
  const statusVisiveis: StatusEquipe[] = ['Disponivel', 'ACaminho', 'EmAtendimento', 'Indisponivel', 'EmPausa', 'DeslocamentoOutraAtividade'];
  return (
    <div className={cn('space-y-2 text-xs', className)} aria-label="Legenda do mapa">
      <div>
        <p className="mb-1 font-semibold text-navy">Ordens de serviço</p>
        <ul className="flex flex-wrap gap-x-3 gap-y-1">
          {prioridades.map((p) => (
            <li key={p.codigo} className="flex items-center gap-1">
              <span
                className={cn('inline-block h-3 w-3 border border-white shadow', p.critica ? 'rotate-45' : 'rounded-full')}
                style={{ background: p.cor ?? '#5b6b80' }}
                aria-hidden
              />
              {p.codigo} · {p.nome}
              {p.critica && <span className="text-muted">(crítica — losango)</span>}
            </li>
          ))}
        </ul>
      </div>
      {mostrarEquipes && (
        <div>
          <p className="mb-1 font-semibold text-navy">Equipes</p>
          <ul className="flex flex-wrap gap-x-3 gap-y-1">
            {statusVisiveis.map((s) => (
              <li key={s} className="flex items-center gap-1">
                <span className="inline-block h-3 w-3 rounded-[2px]" style={{ background: COR_STATUS_EQUIPE[s] }} aria-hidden />
                {STATUS_EQUIPE_LABEL[s]}
              </li>
            ))}
            <li className="flex items-center gap-1">
              <span className="inline-block h-3 w-3 rounded-[2px] outline-1 outline-dashed outline-[#7a5a00]" aria-hidden />
              Posição demonstrativa (tracejado)
            </li>
          </ul>
        </div>
      )}
      {mostrarSubestacoes && (
        <p className="flex items-center gap-1">
          <svg width="12" height="12" viewBox="0 0 18 18" aria-hidden>
            <polygon points="9,1 17,16 1,16" fill="#17365D" />
          </svg>
          Subestação (circuito)
        </p>
      )}
      {semCoordenadas !== undefined && (
        <p className="text-muted">
          <strong className="text-ink">{semCoordenadas}</strong> OS sem coordenadas (não exibidas no mapa).
        </p>
      )}
    </div>
  );
}
