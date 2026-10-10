import type { ReactNode } from 'react';
import { AlertTriangle, Ban, CheckCircle2, MapPin, Route, Star, Users } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { DemoBadge } from '@/components/feedback/demo';
import { formatCoordenada, formatDateTime, formatDistanciaKm } from '@/lib/format';
import { ORIGEM_LOCALIZACAO_EQUIPE_LABEL, STATUS_EQUIPE_LABEL, label } from '@/lib/labels';
import { cn } from '@/lib/utils';
import type { EquipeCandidataDto } from '@/types/api';
import { textoTempoEstimado, type AvaliacaoSelecao } from '@/features/despacho/lib/selecaoEquipe';

/** Uma equipe candidata ao despacho: identificação, compatibilidade, disponibilidade, posição, distância e tempo. */
export function CandidataEquipeItem({
  candidata,
  avaliacao,
  selecionada,
  onSelecionar,
}: {
  candidata: EquipeCandidataDto;
  avaliacao: AvaliacaoSelecao;
  selecionada: boolean;
  onSelecionar: (equipeId: number) => void;
}) {
  const c = candidata;
  const e = c.equipe;
  if (!e) return null;
  const id = `candidata-${e.id}`;
  const demonstrativa = e.origemLocalizacao === 'Demonstrativa';

  return (
    <li
      data-testid="candidata"
      data-recomendada={c.recomendada || undefined}
      className={cn(
        'rounded-md border bg-white text-[13px]',
        c.recomendada ? 'border-moss border-2 bg-moss-soft/40' : 'border-line',
        selecionada && 'ring-2 ring-primary',
        !avaliacao.selecionavel && 'bg-canvas',
      )}
    >
      <label htmlFor={id} className={cn('flex gap-2 px-3 py-2', avaliacao.selecionavel ? 'cursor-pointer' : 'cursor-not-allowed')}>
        <input
          id={id}
          type="radio"
          name="equipe-candidata"
          className="mt-1 h-3.5 w-3.5 accent-[#2457A6]"
          checked={selecionada}
          disabled={!avaliacao.selecionavel}
          onChange={() => onSelecionar(e.id)}
        />
        <div className="min-w-0 flex-1 space-y-1.5">
          <div className="flex flex-wrap items-center gap-1.5">
            <span className="font-semibold text-navy">
              {e.codigo} — {e.nome}
            </span>
            {c.recomendada && (
              <Badge variant="moss">
                <Star aria-hidden /> Recomendada
              </Badge>
            )}
            <Badge variant="neutral">{label(STATUS_EQUIPE_LABEL, e.status)}</Badge>
            {c.apoioIntermunicipal && <Badge variant="navy">Apoio intermunicipal</Badge>}
            {c.compativel ? (
              <Badge variant="success">
                <CheckCircle2 aria-hidden /> Compatível
              </Badge>
            ) : (
              <Badge variant="danger">
                <AlertTriangle aria-hidden /> Incompatível
              </Badge>
            )}
            {c.disponivel ? (
              <Badge variant="success">Disponível</Badge>
            ) : (
              <Badge variant="warning">
                <Ban aria-hidden /> Indisponível{c.motivoIndisponivel ? `: ${c.motivoIndisponivel}` : ''}
              </Badge>
            )}
            <DemoBadge show={e.demonstrativa} label="Equipe demonstrativa" />
          </div>

          {c.justificativa && <p className={cn(c.recomendada ? 'font-medium text-moss' : 'text-muted')}>{c.justificativa}</p>}

          {c.faltando.length > 0 && (
            <ul className="space-y-0.5" aria-label="Requisitos que faltam">
              {c.faltando.map((f) => (
                <li key={f} className="flex items-start gap-1 text-[#7a3306]">
                  <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0" aria-hidden /> Falta — {f}
                </li>
              ))}
            </ul>
          )}

          <dl className="grid gap-x-4 gap-y-0.5 sm:grid-cols-2">
            <Linha rotulo="Município-base">
              {e.municipioBase}
              {e.municipiosAdicionais.length > 0 && <span className="text-muted"> (+ {e.municipiosAdicionais.map((m) => m.nome).join(', ')})</span>}
            </Linha>
            <Linha rotulo="Capacidade">
              {e.despachosAtivos}/{e.capacidade} despacho(s) ativo(s)
            </Linha>
            <Linha rotulo="Distância">
              <span className="inline-flex items-center gap-1">
                <Route className="h-3.5 w-3.5 text-muted" aria-hidden />
                {formatDistanciaKm(c.distanciaKm)}
                {c.origemEstimativa === 'LINHA_RETA' && c.distanciaKm != null && <span className="text-muted">(linha reta)</span>}
              </span>
            </Linha>
            <Linha rotulo="Tempo estimado">
              <span data-testid="tempo-estimado">{textoTempoEstimado(c)}</span>
            </Linha>
            <Linha rotulo="Localização">
              <span className="inline-flex flex-wrap items-center gap-1">
                <MapPin className="h-3.5 w-3.5 text-muted" aria-hidden />
                {e.latitude != null ? formatCoordenada(e.latitude, e.longitude) : 'sem posição conhecida'}
                {e.localizacaoEm && <span className="text-muted">em {formatDateTime(e.localizacaoEm)}</span>}
                <span className={cn(demonstrativa ? 'font-medium text-[#7a5a00]' : 'text-muted')}>
                  · {demonstrativa ? 'posição demonstrativa' : label(ORIGEM_LOCALIZACAO_EQUIPE_LABEL, e.origemLocalizacao)}
                </span>
              </span>
            </Linha>
            <Linha rotulo="Integrantes">
              <span className="inline-flex items-start gap-1">
                <Users className="mt-0.5 h-3.5 w-3.5 shrink-0 text-muted" aria-hidden />
                {e.integrantes.length ? e.integrantes.map((i) => `${i.nome}${i.funcao ? ` (${i.funcao})` : ''}`).join('; ') : '—'}
              </span>
            </Linha>
            <Linha rotulo="Qualificações">{e.qualificacoes.map((q) => q.nome).join(', ') || '—'}</Linha>
            <Linha rotulo="Recursos">{e.recursos.map((r) => r.nome).join(', ') || '—'}</Linha>
          </dl>

          {avaliacao.motivoBloqueio && (
            <p className="flex items-start gap-1 text-xs text-muted" data-testid="motivo-bloqueio">
              <Ban className="mt-0.5 h-3.5 w-3.5 shrink-0" aria-hidden /> {avaliacao.motivoBloqueio}
            </p>
          )}
          {avaliacao.selecionavel && avaliacao.excecao && (
            <p className="text-xs font-medium text-[#7a3306]">Seleção como exceção autorizada — exige justificativa.</p>
          )}
        </div>
      </label>
    </li>
  );
}

function Linha({ rotulo, children }: { rotulo: string; children: ReactNode }) {
  return (
    <div className="flex min-w-0 gap-1">
      <dt className="shrink-0 text-muted">{rotulo}:</dt>
      <dd className="min-w-0 break-words">{children}</dd>
    </div>
  );
}
