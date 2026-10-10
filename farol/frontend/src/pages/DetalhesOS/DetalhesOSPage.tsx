import * as React from 'react';
import { Link, useParams } from 'react-router';
import { ArrowLeft } from 'lucide-react';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { Button } from '@/components/ui/button';
import { NativeSelect, Textarea } from '@/components/ui/input';
import { LoadingState, ErrorState } from '@/components/feedback/states';
import { PriorityBadge } from '@/features/priorizacao/components/PriorityBadge';
import { ClassificacaoBreakdown } from '@/features/priorizacao/components/ClassificacaoBreakdown';
import { useOsDetalhe, useOsMutacao } from '@/features/ordens-servico/hooks/useOsDetalhe';
import { osService } from '@/features/ordens-servico/services/osService';
import { useCatalogo } from '@/hooks/useCatalogo';
import { ROTAS } from '@/app/config/navigation';

export default function DetalhesOSPage() {
  const id = useParams().id ?? null; const os = useOsDetalhe(id);
  const catalogo = useCatalogo();
  const [prioridadeManualId, setPrioridadeManualId] = React.useState('');
  const [justificativa, setJustificativa] = React.useState('');
  const revisar = useOsMutacao(
    (dados: { prioridadeManualId: number | null; justificativa: string }) => osService.reclassificar(id!, dados),
    'Prioridade da OS revisada e registrada no histórico.',
  );
  if (os.isLoading) return <LoadingState label="Carregando OS..." />;
  if (os.error || !os.data) return <ErrorState error={os.error ?? new Error('OS não encontrada.')} />;
  const d = os.data;
  return <div className="space-y-3"><PageHeader title={d.numero ?? 'Ordem de serviço'} description={`${d.municipio} · ${d.tipoOcorrencia} · ${d.status}`} actions={<Link to={ROTAS.painel}><Button variant="secondary"><ArrowLeft /> Voltar à fila</Button></Link>} />
    <Section title="Classificação"><div className="flex flex-wrap items-center gap-3"><PriorityBadge prioridade={d.classificacao?.prioridade ?? null} manual={d.prioridadeManual} /><span className="text-lg font-semibold">{d.classificacao?.pontuacao ?? 0} pontos</span><span>{d.classificacao?.motivoPrincipal ?? 'Sem explicação disponível.'}</span></div>{d.classificacao && <div className="mt-3"><ClassificacaoBreakdown classificacao={d.classificacao} /></div>}</Section>
    {d.acoesDisponiveis.includes('RECLASSIFICAR') && <Section title="Revisar prioridade manualmente" description="A alteração não apaga o cálculo; a justificativa e a decisão ficam no histórico da OS."><div className="grid gap-3 md:grid-cols-[240px_1fr_auto] md:items-end"><label className="grid gap-1 text-sm"><span>Prioridade decidida</span><NativeSelect value={prioridadeManualId} onChange={(e) => setPrioridadeManualId(e.target.value)}><option value="">Usar prioridade calculada</option>{catalogo.data?.prioridades.filter((p) => p.ativo).map((p) => <option key={p.id} value={p.id}>{p.nome} ({p.codigo})</option>)}</NativeSelect></label><label className="grid gap-1 text-sm"><span>Justificativa obrigatória</span><Textarea value={justificativa} onChange={(e) => setJustificativa(e.target.value)} placeholder="Explique por que esta OS deve mudar de prioridade." /></label><Button disabled={!justificativa.trim()} loading={revisar.isPending} onClick={() => revisar.mutate({ prioridadeManualId: prioridadeManualId ? Number(prioridadeManualId) : null, justificativa })}>Aplicar decisão</Button></div>{revisar.error && <p className="mt-2 text-sm text-danger">Não foi possível alterar: {revisar.error.message}</p>}</Section>}
    <Section title="Localização"><p>{[d.logradouro, d.numeroEndereco, d.bairro, d.cep].filter(Boolean).join(', ') || d.enderecoCompleto || 'Localização não informada.'}</p><p className="mt-1 text-muted">{d.rede?.subestacaoNome ? `Circuito: ${d.rede.subestacaoNome}` : ''}{d.rede?.transformadorNumero ? ` · Transformador: ${d.rede.transformadorNumero}` : ''}</p></Section>
    <Section title="Solicitação"><p>{d.solicitacao?.descricao ?? 'Sem descrição.'}</p><p className="mt-2 text-muted">Canal: {d.solicitacao?.canal ?? '—'} · aberta em {new Date(d.abertaEm).toLocaleString('pt-BR')}</p></Section>
    <Section title="Despacho"><p>{d.despachoAtivo ? `Equipe ${d.despachoAtivo.equipe?.codigo ?? d.despachoAtivo.equipe?.nome ?? 'designada'}` : 'Aguardando designação de equipe.'}</p></Section>
    <Section title="Histórico"><ul className="space-y-2">{d.historico.map((h, i) => <li key={`${h.ocorridoEm}-${i}`} className="border-b border-line pb-2"><strong>{h.tipo}</strong> · {new Date(h.ocorridoEm).toLocaleString('pt-BR')}<span className="block text-muted">{h.descricao}</span></li>)}</ul></Section></div>;
}
