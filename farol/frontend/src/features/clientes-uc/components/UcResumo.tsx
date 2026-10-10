import type { UnidadeConsumidoraDto } from '@/types/api';
import { DemoBadge } from '@/components/feedback/demo';
import { Badge } from '@/components/ui/badge';
import { formatCep } from '@/lib/format';
import { SITUACAO_CLIENTE_LABEL } from '@/lib/labels';

/** Dados da UC necessários ao atendimento (cliente mascarado — LGPD). */
export function UcResumo({ uc, rotulosSituacao }: { uc: UnidadeConsumidoraDto; rotulosSituacao?: Record<string, string> | null }) {
  const situacao = uc.situacao ? (rotulosSituacao?.[uc.situacao] ?? SITUACAO_CLIENTE_LABEL[uc.situacao] ?? uc.situacao) : '—';
  const endereco = [
    [uc.logradouro, uc.numeroImovel].filter(Boolean).join(', '),
    uc.bairro,
    uc.municipio,
    uc.cep ? `CEP ${formatCep(uc.cep)}` : null,
  ]
    .filter(Boolean)
    .join(' — ');
  return (
    <dl className="grid grid-cols-1 gap-x-4 gap-y-0.5 text-[13px] sm:grid-cols-[auto_1fr_auto_1fr]">
      <dt className="text-muted">Cliente</dt>
      <dd className="flex items-center gap-1.5">
        {uc.clienteMascarado ?? '—'}
        <DemoBadge show={uc.demonstrativa} label="UC demonstrativa" />
      </dd>
      <dt className="text-muted">Classe</dt>
      <dd>{uc.classe ?? '—'}</dd>
      <dt className="text-muted">Situação</dt>
      <dd>
        <Badge variant={uc.situacao === 'LIGADO' ? 'success' : uc.situacao === 'DESLIGADO' ? 'warning' : 'outline'}>{situacao}</Badge>
      </dd>
      <dt className="text-muted">Transformador</dt>
      <dd className="font-mono">{uc.transformador ?? '—'}</dd>
      <dt className="text-muted">Circuito</dt>
      <dd>{uc.subestacao ?? '—'}</dd>
      <dt className="text-muted">Conjunto</dt>
      <dd>{uc.conjunto ?? '—'}</dd>
      <dt className="text-muted">Endereço da UC</dt>
      <dd className="sm:col-span-3">{endereco || '—'}</dd>
    </dl>
  );
}
