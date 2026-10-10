import { AlertTriangle, CircleHelp, Scale, ShieldAlert } from 'lucide-react';
import type { ClassificacaoDto } from '@/types/api';
import { PriorityBadge } from '@/features/priorizacao/components/PriorityBadge';
import { Badge } from '@/components/ui/badge';
import { DemoBadge } from '@/components/feedback/demo';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { formatDateTime } from '@/lib/format';
import { cn } from '@/lib/utils';

/**
 * Detalhamento explicável de uma classificação (prévia, simulador, detalhe da OS):
 * pontos por critério, regras de precedência aplicadas, motivo principal e versão.
 * Nenhum cálculo é feito aqui — tudo vem da API.
 */
export function ClassificacaoBreakdown({
  classificacao,
  compact,
  className,
  ocultarZerados,
}: {
  classificacao: ClassificacaoDto;
  compact?: boolean;
  className?: string;
  /** Oculta critérios com 0 pontos (útil em painéis estreitos). */
  ocultarZerados?: boolean;
}) {
  const c = classificacao;
  const itens = ocultarZerados ? c.itens.filter((i) => i.pontos !== 0 || i.semPontosDefinidos || i.requerConfirmacao) : c.itens;
  const soma = c.itens.reduce((acc, i) => acc + (i.pontos ?? 0), 0);
  const faixaDifere = c.prioridadePelaFaixa && c.prioridade && c.prioridadePelaFaixa.id !== c.prioridade.id;

  return (
    <div className={cn('space-y-3 text-[13px]', className)}>
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
        <div className="flex items-center gap-2">
          <PriorityBadge prioridade={c.prioridade} />
        </div>
        <div>
          <span className="text-muted">Pontuação total: </span>
          <strong className="text-base tabular-nums text-navy" data-testid="pontuacao-total">
            {c.pontuacao}
          </strong>
        </div>
        <div>
          <span className="text-muted">Nível de precedência: </span>
          <strong className="tabular-nums">{c.nivelPrecedencia}</strong>
        </div>
        {faixaDifere && (
          <div className="flex items-center gap-1 text-muted">
            Pela faixa de pontos seria <PriorityBadge prioridade={c.prioridadePelaFaixa} compact />
          </div>
        )}
      </div>

      {c.motivoPrincipal && (
        <p className="rounded border border-line bg-canvas px-2 py-1.5">
          <span className="font-medium text-navy">Motivo principal: </span>
          {c.motivoPrincipal}
        </p>
      )}

      {c.regrasAplicadas.length > 0 && (
        <div>
          <p className="mb-1 flex items-center gap-1 font-medium text-navy">
            <ShieldAlert className="h-4 w-4" aria-hidden /> Regras de precedência aplicadas (camada A)
          </p>
          <ul className="space-y-1">
            {c.regrasAplicadas.map((r) => (
              <li key={r.codigo ?? r.nome} className="flex flex-wrap items-center gap-2 rounded border border-danger/25 bg-danger-soft px-2 py-1">
                <span className="font-medium">{r.nome}</span>
                <span className="font-mono text-xs text-muted">{r.codigo}</span>
                <Badge variant="outline">nível {r.nivelPrecedencia}</Badge>
                {r.prioridadeMinima && <Badge variant="danger">prioridade mínima: {r.prioridadeMinima}</Badge>}
              </li>
            ))}
          </ul>
        </div>
      )}

      <div>
        <p className="mb-1 flex items-center gap-1 font-medium text-navy">
          <Scale className="h-4 w-4" aria-hidden /> Pontuação por critério (camada B)
        </p>
        <Table containerClassName={compact ? 'max-h-72' : undefined}>
          <THead>
            <TR>
              <TH>Critério</TH>
              <TH>Resposta considerada</TH>
              <TH className="text-right">Pontos</TH>
            </TR>
          </THead>
          <TBody>
            {itens.map((i) => (
              <TR key={i.criterioCodigo ?? i.criterio}>
                <TD className="font-medium">{i.criterio}</TD>
                <TD>
                  <div className="flex flex-wrap items-center gap-1">
                    <span>{i.opcoes.length ? i.opcoes.join(', ') : '—'}</span>
                    {i.requerConfirmacao && (
                      <Badge variant="warning" title="Informação desconhecida ou que requer confirmação">
                        <CircleHelp aria-hidden /> requer confirmação
                      </Badge>
                    )}
                    {i.semPontosDefinidos && (
                      <Badge variant="danger">
                        <AlertTriangle aria-hidden /> sem pontos definidos
                      </Badge>
                    )}
                  </div>
                </TD>
                <TD className="text-right font-semibold tabular-nums">{i.semPontosDefinidos ? '—' : i.pontos}</TD>
              </TR>
            ))}
            <TR className="bg-canvas">
              <TD colSpan={2} className="text-right font-semibold">
                Total {ocultarZerados && itens.length !== c.itens.length ? '(todos os critérios)' : ''}
              </TD>
              <TD className="text-right font-bold tabular-nums">{soma}</TD>
            </TR>
          </TBody>
        </Table>
      </div>

      {c.alertas.length > 0 && (
        <ul className="space-y-1">
          {c.alertas.map((a, idx) => (
            <li key={idx} className="flex items-start gap-1.5 text-[#7a3306]">
              <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden /> {a}
            </li>
          ))}
        </ul>
      )}

      {c.versao && (
        <p className="flex flex-wrap items-center gap-2 text-xs text-muted">
          <span>
            Versão de pontuação nº {c.versao.numero} —{' '}
            {c.versao.municipioId ? (
              <strong className="text-ink">específica de {c.versao.municipioNome ?? 'município'}</strong>
            ) : (
              'global'
            )}
          </span>
          {c.versao.vigenciaInicio && <span>vigente desde {formatDateTime(c.versao.vigenciaInicio)}</span>}
          <DemoBadge show={c.versao.demonstrativa} label="Pontuação demonstrativa" />
          {c.calculadoEm && <span>· calculado em {formatDateTime(c.calculadoEm, true)}</span>}
        </p>
      )}
    </div>
  );
}
