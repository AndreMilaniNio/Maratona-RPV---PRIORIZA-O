import * as React from 'react';
import { CheckCircle2, Copy, ExternalLink, MapPinned, Navigation } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DescriptionList, DItem } from '@/components/ui/misc';
import { InlineAlert } from '@/components/feedback/states';
import { notificarErro, notificarSucesso } from '@/components/feedback/toast';
import { formatCep, formatCoordenada } from '@/lib/format';
import { copiarTexto } from '@/lib/utils';
import type { DesignacaoDto } from '@/types/api';

/**
 * Dados entregues à equipe após a designação: coordenadas (copiar / abrir no mapa),
 * endereço, CEP, UCs e rede. Sem coordenadas → aviso de localização aproximada.
 */
export function EntregaPanel({ designacao }: { designacao: DesignacaoDto }) {
  const e = designacao.entrega;
  const [copiado, setCopiado] = React.useState(false);
  const temCoord = e?.latitude != null && e?.longitude != null;
  const textoCoord = e?.coordenadas ?? (temCoord ? formatCoordenada(e!.latitude, e!.longitude) : '');

  const copiar = async () => {
    if (!textoCoord) return;
    const ok = await copiarTexto(textoCoord);
    if (ok) {
      setCopiado(true);
      notificarSucesso('Coordenadas copiadas', textoCoord);
      setTimeout(() => setCopiado(false), 2500);
    } else {
      notificarErro(new Error('Não foi possível copiar automaticamente. Selecione o texto e copie manualmente.'));
    }
  };

  return (
    <div className="space-y-3" data-testid="entrega-panel">
      <InlineAlert tone="success" icon={<CheckCircle2 />} title="Equipe designada">
        OS <strong>{designacao.osNumero}</strong> disponibilizada para{' '}
        <strong>
          {designacao.equipe?.codigo} — {designacao.equipe?.nome}
        </strong>
        . Repasse à equipe os dados abaixo.
      </InlineAlert>

      {e?.localizacaoAproximada && (
        <InlineAlert tone="warning" title="Localização aproximada" className="text-[13px]">
          {e.avisoLocalizacao ?? 'A OS não tem coordenadas: a localização é aproximada ou depende de confirmação no local.'}
        </InlineAlert>
      )}

      {e ? (
        <section className="rounded-md border border-line bg-white px-4 py-3" aria-label="Dados entregues à equipe">
          <div className="mb-3 flex flex-wrap items-center gap-2">
            <span className="font-mono text-[15px] font-semibold text-navy" data-testid="entrega-coordenadas">
              {textoCoord || 'Sem coordenadas'}
            </span>
            {textoCoord && (
              <Button size="sm" variant="secondary" onClick={copiar} aria-label="Copiar coordenadas">
                <Copy /> {copiado ? 'Copiado' : 'Copiar coordenadas'}
              </Button>
            )}
            {e.linkMapa && (
              <Button size="sm" variant="outline" asChild>
                <a href={e.linkMapa} target="_blank" rel="noopener noreferrer">
                  <ExternalLink /> Abrir no mapa
                </a>
              </Button>
            )}
            {e.geoUri && (
              <Button size="sm" variant="ghost" asChild>
                <a href={e.geoUri}>
                  <Navigation /> Abrir no app de mapas (geo:)
                </a>
              </Button>
            )}
          </div>
          <DescriptionList>
            <DItem label="Endereço">
              <span className="inline-flex items-start gap-1">
                <MapPinned className="mt-0.5 h-3.5 w-3.5 shrink-0 text-muted" aria-hidden /> {e.endereco ?? '—'}
              </span>
            </DItem>
            <DItem label="CEP">{formatCep(e.cep)}</DItem>
            <DItem label="UC(s)">{e.ucs.length ? e.ucs.join(', ') : 'UC não informada'}</DItem>
            <DItem label="Circuito / subestação">{e.subestacao ?? '—'}</DItem>
            <DItem label="Conjunto elétrico">{e.conjunto ?? '—'}</DItem>
            <DItem label="Transformador">{e.transformador ?? '—'}</DItem>
          </DescriptionList>
        </section>
      ) : (
        <p className="text-[13px] text-muted">A API não devolveu os dados de entrega.</p>
      )}
    </div>
  );
}
