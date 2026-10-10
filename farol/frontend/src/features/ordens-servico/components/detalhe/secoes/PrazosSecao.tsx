import { CalendarClock } from 'lucide-react';
import { Section } from '@/components/layout/PageHeader';
import { DemoBadge } from '@/components/feedback/demo';
import { Badge } from '@/components/ui/badge';
import { Table, TBody, TD, TH, THead, TR } from '@/components/ui/table';
import { PrazoRestante, useAgora } from '@/features/ordens-servico/components/PrazoRestante';
import { formatDateTime, formatPrazoRestante } from '@/lib/format';
import { CALENDARIO_LABEL, TIPO_PRAZO_LABEL } from '@/lib/labels';
import { cn } from '@/lib/utils';
import type { CalendarioPrazo, PrazosDto, TipoPrazo } from '@/types/api';

const ORDEM: { tipo: TipoPrazo; campo: keyof Pick<PrazosDto, 'triagem' | 'despacho' | 'inicio' | 'restabelecimento' | 'conclusao'> }[] = [
  { tipo: 'Triagem', campo: 'triagem' },
  { tipo: 'Despacho', campo: 'despacho' },
  { tipo: 'Inicio', campo: 'inicio' },
  { tipo: 'Restabelecimento', campo: 'restabelecimento' },
  { tipo: 'Conclusao', campo: 'conclusao' },
];

/** Os cinco prazos do código de prioridade, com o próximo destacado. */
export function PrazosSecao({ prazos }: { prazos: PrazosDto | null }) {
  const agora = useAgora();
  return (
    <Section
      title="Prazos de atendimento"
      icon={<CalendarClock />}
      actions={
        prazos && (
          <div className="flex items-center gap-2 text-xs text-muted">
            {prazos.calendario && <span>Calendário: {CALENDARIO_LABEL[prazos.calendario as CalendarioPrazo] ?? prazos.calendario}</span>}
            <DemoBadge show={prazos.demonstrativos} label="Prazos demonstrativos" />
          </div>
        )
      }
    >
      {!prazos ? (
        <p className="text-[13px] text-muted">Sem prazos calculados.</p>
      ) : (
        <Table>
          <THead>
            <TR>
              <TH>Prazo</TH>
              <TH>Limite</TH>
              <TH>Situação</TH>
            </TR>
          </THead>
          <TBody>
            {ORDEM.map(({ tipo, campo }) => {
              const limite = prazos[campo];
              const proximo = prazos.proximo?.tipo === tipo;
              const f = formatPrazoRestante(limite ? { limite } : null, agora);
              return (
                <TR key={tipo} className={cn(proximo && 'bg-primary-soft font-medium')} aria-current={proximo || undefined}>
                  <TD>
                    {TIPO_PRAZO_LABEL[tipo]} {proximo && <Badge variant="primary">próximo</Badge>}
                  </TD>
                  <TD className="tabular-nums">{limite ? formatDateTime(limite) : 'Não se aplica'}</TD>
                  <TD>
                    {proximo && prazos.proximo ? (
                      <PrazoRestante prazo={prazos.proximo} agora={agora} />
                    ) : limite ? (
                      // Só o próximo prazo pendente é acompanhado; os demais são informativos.
                      <span className="text-muted">{f.texto}</span>
                    ) : (
                      '—'
                    )}
                  </TD>
                </TR>
              );
            })}
          </TBody>
        </Table>
      )}
    </Section>
  );
}
