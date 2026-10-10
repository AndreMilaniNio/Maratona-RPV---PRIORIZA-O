import { FileText, Link2 } from 'lucide-react';
import { Section } from '@/components/layout/PageHeader';
import { Badge } from '@/components/ui/badge';
import { DescriptionList, DItem } from '@/components/ui/misc';
import { formatDateTime } from '@/lib/format';
import { CANAL_LABEL, label } from '@/lib/labels';
import type { OrdemServicoDetalheDto, SolicitacaoDto } from '@/types/api';

/** Identificação da solicitação original e das solicitações vinculadas (unificação). */
export function SolicitacaoSecao({ os }: { os: OrdemServicoDetalheDto }) {
  return (
    <Section title="Solicitação" icon={<FileText />}>
      {os.solicitacao ? <SolicitacaoBloco s={os.solicitacao} /> : <p className="text-[13px] text-muted">Solicitação não disponível.</p>}
      {os.solicitacoesVinculadas.length > 0 && (
        <div className="mt-3 border-t border-line pt-2">
          <p className="mb-1 flex items-center gap-1 text-[13px] font-medium text-navy">
            <Link2 className="h-4 w-4" aria-hidden /> Solicitações vinculadas ({os.solicitacoesVinculadas.length})
          </p>
          <ul className="space-y-2">
            {os.solicitacoesVinculadas.map((s) => (
              <li key={s.id} className="rounded border border-line px-2 py-1.5">
                <SolicitacaoBloco s={s} compacta />
              </li>
            ))}
          </ul>
        </div>
      )}
    </Section>
  );
}

function SolicitacaoBloco({ s, compacta }: { s: SolicitacaoDto; compacta?: boolean }) {
  return (
    <div className="space-y-2">
      <DescriptionList>
        <DItem label="Solicitação">
          <span className="font-mono">{s.numero}</span>
        </DItem>
        <DItem label="Canal">{label(CANAL_LABEL, s.canal)}</DItem>
        <DItem label="Origem">{s.origem ?? '—'}</DItem>
        <DItem label="Protocolo externo">{s.protocoloExterno ?? '—'}</DItem>
        <DItem label="Registrada em">
          {formatDateTime(s.registradaEm)} {s.registradaPor && <span className="text-muted">por {s.registradaPor}</span>}
        </DItem>
        <DItem label="UC">
          {s.ucNaoInformada ? (
            <span>
              <Badge variant="warning">UC não informada</Badge>
              {s.motivoUcNaoInformada && <span className="ml-1">— {s.motivoUcNaoInformada}</span>}
            </span>
          ) : s.ucs.length ? (
            s.ucs.map((u) => (
              <span key={u.numero} className="mr-2 inline-flex items-center gap-1">
                <span className="font-mono">{u.numero}</span>
                {!u.validadaNoCadastro && <Badge variant="warning">não validada</Badge>}
              </span>
            ))
          ) : (
            '—'
          )}
        </DItem>
      </DescriptionList>
      {!compacta || s.descricao ? (
        <div>
          <p className="text-[13px] text-muted">Descrição original</p>
          <blockquote className="whitespace-pre-wrap rounded border-l-4 border-primary/40 bg-canvas px-3 py-1.5 text-[13px]">
            {s.descricao || '—'}
          </blockquote>
        </div>
      ) : null}
    </div>
  );
}
