import { Ban, MapPin, UserCog } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { InlineAlert } from '@/components/feedback/states';
import { PriorityBadge } from '@/features/priorizacao/components/PriorityBadge';
import { PrazoRestante, useAgora } from '@/features/ordens-servico/components/PrazoRestante';
import { formatDateTime } from '@/lib/format';
import { STATUS_OS_LABEL, TIPO_MANUTENCAO_LABEL, label } from '@/lib/labels';
import type { OrdemServicoDetalheDto } from '@/types/api';

/** Cabeçalho do detalhe: número, prioridade, pontuação, status, posição, prazos e datas. */
export function OsCabecalho({ os }: { os: OrdemServicoDetalheDto }) {
  const agora = useAgora();
  const c = os.classificacao;
  return (
    <div className="space-y-2" data-testid="os-cabecalho">
      <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5">
        <span className="font-mono text-lg font-bold text-navy">{os.numero}</span>
        <PriorityBadge prioridade={c?.prioridade} manual={os.prioridadeManual} />
        <span className="text-[13px]">
          <span className="text-muted">Pontuação </span>
          <strong className="tabular-nums">{c?.pontuacao ?? '—'}</strong>
        </span>
        <Badge variant="navy">{label(STATUS_OS_LABEL, os.status)}</Badge>
        {os.posicao != null && (
          <Badge variant="primary" title="Posição na fila do município">
            {os.posicao}º na fila de {os.municipio}
          </Badge>
        )}
        {os.prazos?.proximo && <PrazoRestante prazo={os.prazos.proximo} agora={agora} mostrarTipo />}
      </div>
      <div className="flex flex-wrap gap-x-4 gap-y-1 text-[13px]">
        <span className="inline-flex items-center gap-1">
          <MapPin className="h-3.5 w-3.5 text-muted" aria-hidden /> {os.municipio}
        </span>
        <span>
          <span className="text-muted">Manutenção: </span>
          {label(TIPO_MANUTENCAO_LABEL, os.tipoManutencao)}
        </span>
        <span>
          <span className="text-muted">Ocorrência: </span>
          {os.tipoOcorrencia ?? '—'}
        </span>
        <span>
          <span className="text-muted">Aberta em </span>
          {formatDateTime(os.abertaEm)}
        </span>
        <span>
          <span className="text-muted">Atualizada em </span>
          {formatDateTime(os.atualizadaEm)}
        </span>
        {os.encerradaEm && (
          <span>
            <span className="text-muted">Encerrada em </span>
            {formatDateTime(os.encerradaEm)}
          </span>
        )}
      </div>
      {os.prioridadeManual && (
        <InlineAlert tone="info" icon={<UserCog />} title="Prioridade definida manualmente">
          {os.justificativaManual ?? 'Sem justificativa registrada.'}
          {c?.prioridadePelaFaixa && (
            <span className="ml-1 text-muted">(pela faixa de pontos seria{c.prioridadePelaFaixa.codigo} · {c.prioridadePelaFaixa.nome})</span>
          )}
        </InlineAlert>
      )}
      {os.motivoCancelamento && (
        <InlineAlert tone="danger" icon={<Ban />} title="Motivo do cancelamento">
          {os.motivoCancelamento}
        </InlineAlert>
      )}
    </div>
  );
}
