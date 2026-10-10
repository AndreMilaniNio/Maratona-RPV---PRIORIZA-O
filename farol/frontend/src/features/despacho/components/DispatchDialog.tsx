import * as React from 'react';
import { RefreshCw, Truck } from 'lucide-react';
import { Dialog, DialogBody, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/input';
import { FormField } from '@/components/forms/FormField';
import { EmptyState, ErrorState, InlineAlert, LoadingState } from '@/components/feedback/states';
import { notificarSucesso } from '@/components/feedback/toast';
import { usePermissao } from '@/features/autenticacao/hooks/AuthProvider';
import { PERMISSOES } from '@/lib/permissions';
import { ApiError } from '@/services/api/client';
import { useOsDetalhe } from '@/features/ordens-servico/hooks/useOsDetalhe';
import { useCandidatas, useDesignar } from '@/features/despacho/hooks/useDespacho';
import { avaliarSelecao, ordenarCandidatas } from '@/features/despacho/lib/selecaoEquipe';
import { despachoSchema } from '@/features/despacho/schemas/despachoSchema';
import { CandidataEquipeItem } from '@/features/despacho/components/CandidataEquipeItem';
import { CandidatasMapa } from '@/features/despacho/components/CandidatasMapa';
import { EntregaPanel } from '@/features/despacho/components/EntregaPanel';
import { RequisitosResumo } from '@/features/despacho/components/RequisitosResumo';
import type { DesignacaoDto } from '@/types/api';

export interface DispatchDialogProps {
  /** OS a despachar; null = fechado. */
  osId: string | null;
  onOpenChange: (open: boolean) => void;
  /** Chamado após designação confirmada. */
  onDespachado?: () => void;
}

const MSG_409 = 'A equipe deixou de estar disponível (ou a OS já foi despachada). A lista foi atualizada.';

/** "Disponibilizar para equipe" (seção 5.5): candidatas, recomendação, mapa, confirmação e entrega. */
export function DispatchDialog({ osId, onOpenChange, onDespachado }: DispatchDialogProps) {
  return (
    <Dialog open={!!osId} onOpenChange={onOpenChange}>
      <DialogContent size="full" className="h-[92vh]">
        {osId && <DispatchConteudo key={osId} osId={osId} onFechar={() => onOpenChange(false)} onDespachado={onDespachado} />}
      </DialogContent>
    </Dialog>
  );
}

function DispatchConteudo({ osId, onFechar, onDespachado }: { osId: string; onFechar: () => void; onDespachado?: () => void }) {
  const podeExcecao = usePermissao(PERMISSOES.despachoExcecao);
  const [incluirApoio, setIncluirApoio] = React.useState(false);
  const [selecionadaId, setSelecionadaId] = React.useState<number | null>(null);
  const [justificativa, setJustificativa] = React.useState('');
  const [erroJustificativa, setErroJustificativa] = React.useState<string | null>(null);
  const [erroApi, setErroApi] = React.useState<unknown>(null);
  const [aviso409, setAviso409] = React.useState(false);
  const [designacao, setDesignacao] = React.useState<DesignacaoDto | null>(null);

  const os = useOsDetalhe(osId);
  const cand = useCandidatas(osId, incluirApoio);
  const designar = useDesignar(osId);

  const lista = React.useMemo(() => ordenarCandidatas(cand.data?.equipes ?? []), [cand.data]);
  const selecionada = lista.find((c) => c.equipe?.id === selecionadaId) ?? null;
  const avaliacaoSel = selecionada ? avaliarSelecao(selecionada, podeExcecao) : null;

  // A lista mudou (atualização / apoio): desfaz a seleção se a equipe sumiu ou deixou de ser selecionável.
  React.useEffect(() => {
    if (selecionadaId == null || !cand.data) return;
    const c = cand.data.equipes.find((x) => x.equipe?.id === selecionadaId);
    if (!c || !avaliarSelecao(c, podeExcecao).selecionavel) setSelecionadaId(null);
  }, [cand.data, selecionadaId, podeExcecao]);

  const selecionar = (id: number) => {
    const c = lista.find((x) => x.equipe?.id === id);
    if (!c || !avaliarSelecao(c, podeExcecao).selecionavel) return;
    setSelecionadaId(id);
    setErroApi(null);
    setErroJustificativa(null);
  };

  const confirmar = async () => {
    if (!selecionada?.equipe || !avaliacaoSel) return;
    const parsed = despachoSchema.safeParse({ justificativa, exigeJustificativa: avaliacaoSel.exigeJustificativa });
    if (!parsed.success) {
      setErroJustificativa(parsed.error.issues[0]?.message ?? 'Justificativa inválida.');
      return;
    }
    setErroJustificativa(null);
    setErroApi(null);
    setAviso409(false);
    try {
      const r = await designar.mutateAsync({
        equipeId: selecionada.equipe.id,
        justificativa: justificativa.trim() || null,
        excecao: avaliacaoSel.excecao,
        apoioIntermunicipal: selecionada.apoioIntermunicipal,
      });
      setDesignacao(r);
      notificarSucesso('Equipe designada', `${r.equipe?.codigo ?? ''} → OS ${r.osNumero ?? ''}`);
      onDespachado?.();
    } catch (e) {
      if (e instanceof ApiError && e.status === 409) {
        setAviso409(true);
        setSelecionadaId(null);
        void cand.refetch();
      } else {
        if (e instanceof ApiError && e.fieldErrors.justificativa) setErroJustificativa(e.fieldErrors.justificativa.join(' '));
        setErroApi(e);
      }
    }
  };

  const numero = cand.data?.osNumero ?? os.data?.numero ?? '';

  return (
    <>
      <DialogHeader>
        <DialogTitle className="flex items-center gap-2">
          <Truck className="h-4 w-4" aria-hidden /> Disponibilizar para equipe {numero && `— OS ${numero}`}
        </DialogTitle>
        <DialogDescription>
          {designacao
            ? 'Designação registrada. Dados de localização para a equipe.'
            : 'Compatibilidade técnica e segurança vêm antes da distância. A equipe recomendada aparece primeiro.'}
        </DialogDescription>
      </DialogHeader>

      <DialogBody className="space-y-3">
        {designacao ? (
          <EntregaPanel designacao={designacao} />
        ) : cand.isLoading ? (
          <LoadingState label="Carregando equipes candidatas…" />
        ) : cand.isError && !cand.data ? (
          <ErrorState error={cand.error} onRetry={() => void cand.refetch()} title="Não foi possível carregar as equipes candidatas" />
        ) : cand.data ? (
          <>
            {aviso409 && (
              <InlineAlert tone="danger" title="Designação não realizada">
                {MSG_409}
              </InlineAlert>
            )}
            <div className="flex flex-wrap items-start justify-between gap-3">
              <RequisitosResumo dados={cand.data} />
              <div className="flex items-center gap-3">
                <div className="flex items-center gap-2">
                  <Switch id="incluir-apoio" checked={incluirApoio} onCheckedChange={setIncluirApoio} />
                  <Label htmlFor="incluir-apoio">Incluir apoio intermunicipal</Label>
                </div>
                <Button size="sm" variant="secondary" onClick={() => void cand.refetch()} loading={cand.isFetching}>
                  <RefreshCw /> Atualizar lista
                </Button>
              </div>
            </div>
            {!podeExcecao && (
              <p className="text-xs text-muted">
                Equipes incompatíveis ou indisponíveis aparecem na lista, mas só podem ser designadas como exceção por quem tem a permissão de
                exceção (supervisor).
              </p>
            )}
            <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_minmax(320px,440px)]">
              <div>
                {lista.length === 0 ? (
                  <EmptyState
                    title="Nenhuma equipe candidata"
                    description={incluirApoio ? 'Nenhuma equipe ativa encontrada.' : 'Tente incluir o apoio intermunicipal.'}
                  />
                ) : (
                  <ul className="space-y-2" role="radiogroup" aria-label="Equipes candidatas">
                    {lista.map((c) => (
                      <CandidataEquipeItem
                        key={c.equipe?.id}
                        candidata={c}
                        avaliacao={avaliarSelecao(c, podeExcecao)}
                        selecionada={c.equipe?.id === selecionadaId}
                        onSelecionar={selecionar}
                      />
                    ))}
                  </ul>
                )}
              </div>
              <div className="lg:sticky lg:top-0 lg:self-start">
                <CandidatasMapa
                  dados={cand.data}
                  candidatas={lista}
                  selecionadaId={selecionadaId}
                  onSelecionar={selecionar}
                  prioridade={os.data?.classificacao?.prioridade}
                  height={400}
                />
              </div>
            </div>
          </>
        ) : null}
      </DialogBody>

      <DialogFooter className="justify-between">
        {designacao ? (
          <Button onClick={onFechar}>Fechar</Button>
        ) : (
          <div className="flex w-full flex-col gap-2">
            {selecionada?.equipe && avaliacaoSel && (
              <div className="grid gap-2 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-end">
                <FormField
                  id="justificativa-despacho"
                  label={
                    avaliacaoSel.exigeJustificativa
                      ? `Justificativa (${[avaliacaoSel.excecao && 'exceção', selecionada.apoioIntermunicipal && 'apoio intermunicipal'].filter(Boolean).join(' e ')})`
                      : 'Justificativa (opcional)'
                  }
                  required={avaliacaoSel.exigeJustificativa}
                  error={erroJustificativa}
                  hint={`Equipe selecionada: ${selecionada.equipe.codigo} — ${selecionada.equipe.nome}`}
                >
                  <Textarea rows={2} className="min-h-14" value={justificativa} onChange={(e) => setJustificativa(e.target.value)} />
                </FormField>
              </div>
            )}
            {erroApi != null && <ErrorState error={erroApi} compact />}
            <div className="flex flex-wrap justify-end gap-2">
              <Button variant="secondary" onClick={onFechar} disabled={designar.isPending}>
                Cancelar
              </Button>
              <Button
                variant={avaliacaoSel?.excecao ? 'danger' : 'primary'}
                onClick={confirmar}
                disabled={!selecionada}
                loading={designar.isPending}
              >
                {avaliacaoSel?.excecao ? 'Confirmar designação por exceção' : 'Confirmar designação'}
              </Button>
            </div>
          </div>
        )}
      </DialogFooter>
    </>
  );
}
