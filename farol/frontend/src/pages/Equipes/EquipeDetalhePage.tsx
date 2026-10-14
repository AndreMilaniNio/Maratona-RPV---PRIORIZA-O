import { Link, useParams } from 'react-router';
import { ArrowLeft } from 'lucide-react';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { Button } from '@/components/ui/button';
import { LoadingState, ErrorState } from '@/components/feedback/states';
import { useEquipe, useAlterarStatusEquipe } from '@/features/equipes/hooks/useEquipes';
import { StatusEquipeValores } from '@/types/api';
import { STATUS_EQUIPE_LABEL } from '@/lib/labels';
import { ROTAS } from '@/app/config/navigation';

export default function EquipeDetalhePage() { const id = Number(useParams().id); const equipe = useEquipe(Number.isFinite(id) ? id : null); const status = useAlterarStatusEquipe(id);
  if (equipe.isLoading) return <LoadingState label="Carregando equipe..." />; if (equipe.error || !equipe.data) return <ErrorState error={equipe.error ?? new Error('Equipe não encontrada.')} />; const e = equipe.data;
  return <div className="space-y-3"><PageHeader title={`${e.codigo} — ${e.nome}`} description={`${e.municipioBase} · capacidade ${e.capacidade} · ${e.despachosAtivos} despacho(s) ativo(s)`} actions={<Link to={ROTAS.equipes}><Button variant="secondary"><ArrowLeft /> Equipes</Button></Link>} />
    <Section title="Situação operacional"><div className="flex flex-wrap items-center gap-2"><span className="font-medium">Status atual: {STATUS_EQUIPE_LABEL[e.status]}</span>{StatusEquipeValores.map((s) => <Button key={s} size="sm" variant={s === e.status ? 'primary' : 'secondary'} loading={status.isPending && s === e.status} onClick={() => status.mutate({ status: s, justificativa: 'Atualização operacional pela tela de equipes.' })}>{STATUS_EQUIPE_LABEL[s]}</Button>)}</div></Section>
    <Section title="Integrantes"><ul className="space-y-1">{e.integrantes.map((i) => <li key={i.id}>{i.nome} {i.funcao ? `— ${i.funcao}` : ''} {i.matricula ? `(${i.matricula})` : ''}</li>)}</ul></Section>
    <Section title="Qualificações e recursos"><p>{e.qualificacoes.map((x) => x.nome).join(', ') || 'Nenhuma qualificação informada.'}</p><p className="mt-2">{e.recursos.map((x) => x.nome).join(', ') || 'Nenhum recurso informado.'}</p></Section>
    <Section title="Ordens atribuídas">{e.osAtribuidas.length ? <ul className="space-y-1">{e.osAtribuidas.map((o) => <li key={o.osId}><Link className="text-primary hover:underline" to={ROTAS.os(o.osId)}>{o.numero}</Link> — {o.status}</li>)}</ul> : <p className="text-muted">Nenhuma OS atribuída.</p>}</Section></div>; }
