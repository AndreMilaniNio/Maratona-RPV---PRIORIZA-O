import * as React from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ArrowDown, ArrowUp, Plus, Save, Trash2 } from 'lucide-react';
import { ErrorState, LoadingState } from '@/components/feedback/states';
import { notificarErro, notificarSucesso } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import { CheckboxField } from '@/components/ui/checkbox';
import { Input, Textarea } from '@/components/ui/input';
import { Section } from '@/components/layout/PageHeader';
import { api } from '@/services/api/client';
import { queryKeys } from '@/services/api/queryKeys';
import type { CriterioAdminDto, CriterioSalvarRequest } from '@/types/api';

type OpcaoEdicao = { codigo: string; rotulo: string; representaDesconhecido: boolean };
const codigo = (texto: string) => texto.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toUpperCase().replace(/[^A-Z0-9]+/g, '_').replace(/^_|_$/g, '').slice(0, 40);

export function ConstrutorCasos({ municipioId }: { municipioId: number | null }) {
  const cliente = useQueryClient();
  const [id, setId] = React.useState<number | null>(null);
  const [codigoCaso, setCodigoCaso] = React.useState<string | null>(null);
  const [nome, setNome] = React.useState('');
  const [descricao, setDescricao] = React.useState('');
  const [multipla, setMultipla] = React.useState(false);
  const [opcoes, setOpcoes] = React.useState<OpcaoEdicao[]>([{ codigo: '', rotulo: '', representaDesconhecido: false }, { codigo: 'NAO_INFORMADO', rotulo: 'Nao informado', representaDesconhecido: true }]);
  const casos = useQuery({ queryKey: queryKeys.criterios, queryFn: () => api.get<CriterioAdminDto[]>('/api/configuracoes/criterios') });
  const salvar = useMutation({
    mutationFn: (body: CriterioSalvarRequest) => id === null ? api.post<CriterioAdminDto>('/api/admin/criterios', body) : api.put<CriterioAdminDto>(`/api/admin/criterios/${id}`, body),
    onSuccess: () => { notificarSucesso('Caso salvo. Abra ou atualize o rascunho de pontuacao para definir seus pontos e publicar a versao.'); limpar(); void cliente.invalidateQueries({ queryKey: queryKeys.criterios }); void cliente.invalidateQueries({ queryKey: ['catalogo'] }); },
    onError: (erro) => notificarErro(erro, 'Nao foi possivel salvar o caso'),
  });
  const limpar = () => { setId(null); setCodigoCaso(null); setNome(''); setDescricao(''); setMultipla(false); setOpcoes([{ codigo: '', rotulo: '', representaDesconhecido: false }, { codigo: 'NAO_INFORMADO', rotulo: 'Nao informado', representaDesconhecido: true }]); };
  const editar = (caso: CriterioAdminDto) => { setId(caso.id); setCodigoCaso(caso.codigo ?? null); setNome(caso.nome ?? ''); setDescricao(caso.descricao ?? ''); setMultipla(caso.multiplaEscolha); setOpcoes(caso.opcoes.map((o) => ({ codigo: o.codigo ?? '', rotulo: o.rotulo ?? '', representaDesconhecido: o.representaDesconhecido }))); };
  const mover = (indice: number, direcao: -1 | 1) => setOpcoes((atual) => { const destino = indice + direcao; if (destino < 0 || destino >= atual.length) return atual; const copia = [...atual]; [copia[indice], copia[destino]] = [copia[destino], copia[indice]]; return copia; });
  const enviar = () => salvar.mutate({ codigo: codigoCaso ?? `CASO_${codigo(nome)}`, nome, descricao: descricao || null, regraAplicacao: 'Respondido pelo atendente no formulario.', agregacao: multipla ? 'Soma' : 'Maximo', multiplaEscolha: multipla, opcoes, municipiosIds: municipioId === null ? [] : [municipioId], ativo: true });

  return <Section title="Construtor de Casos" description="Crie perguntas e alternativas sem alterar codigo. Depois defina os pontos no rascunho e publique a versao.">
    <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
      <div className="rounded border border-line p-2"><p className="mb-2 text-xs font-medium text-muted">Casos existentes</p>{casos.isLoading ? <LoadingState label="Carregando..." /> : casos.error ? <ErrorState error={casos.error} compact /> : <div className="space-y-1">{casos.data?.filter((c) => c.tipo === 'Personalizado').map((caso) => <button type="button" key={caso.id} onClick={() => editar(caso)} className="w-full rounded px-2 py-1.5 text-left text-[13px] hover:bg-primary-soft">{caso.nome}</button>)}{casos.data?.every((c) => c.tipo !== 'Personalizado') && <p className="text-xs text-muted">Nenhum caso criado.</p>}</div>}</div>
      <div className="space-y-3"><div className="grid gap-3 md:grid-cols-2"><label className="grid gap-1 text-[13px]"><span>Nome do caso</span><Input value={nome} onChange={(e) => setNome(e.target.value)} placeholder="Ex.: Chamado para o setor de TI" /></label><CheckboxField id="caso-multipla" label="Permite mais de uma resposta" checked={multipla} onCheckedChange={setMultipla} /></div><label className="grid gap-1 text-[13px]"><span>Descricao (opcional)</span><Textarea value={descricao} onChange={(e) => setDescricao(e.target.value)} /></label>
        <div className="space-y-2"><div className="flex items-center justify-between"><p className="text-sm font-medium">Alternativas</p><Button size="sm" variant="secondary" onClick={() => setOpcoes((atual) => [...atual, { codigo: '', rotulo: '', representaDesconhecido: false }])}><Plus /> Adicionar</Button></div>{opcoes.map((opcao, indice) => <div className="grid grid-cols-[1fr_1fr_auto_auto_auto] items-center gap-2" key={indice}><Input value={opcao.rotulo} onChange={(e) => setOpcoes((atual) => atual.map((x, i) => i === indice ? { ...x, rotulo: e.target.value, codigo: x.codigo || codigo(e.target.value) } : x))} placeholder="Rotulo" /><Input value={opcao.codigo} onChange={(e) => setOpcoes((atual) => atual.map((x, i) => i === indice ? { ...x, codigo: codigo(e.target.value) } : x))} placeholder="CODIGO" /><CheckboxField id={`desconhecida-${indice}`} label="Desconhecida" checked={opcao.representaDesconhecido} onCheckedChange={(v) => setOpcoes((atual) => atual.map((x, i) => ({ ...x, representaDesconhecido: i === indice ? v : v ? false : x.representaDesconhecido })))} /><Button size="icon-sm" variant="ghost" aria-label="Subir alternativa" onClick={() => mover(indice, -1)}><ArrowUp /></Button><div className="flex"><Button size="icon-sm" variant="ghost" aria-label="Descer alternativa" onClick={() => mover(indice, 1)}><ArrowDown /></Button><Button size="icon-sm" variant="ghost" aria-label="Remover alternativa" disabled={opcoes.length <= 2} onClick={() => setOpcoes((atual) => atual.filter((_, i) => i !== indice))}><Trash2 /></Button></div></div>)}</div>
        <div className="flex justify-end gap-2"><Button variant="secondary" onClick={limpar}>Novo</Button><Button onClick={enviar} loading={salvar.isPending} disabled={!nome.trim() || opcoes.some((o) => !o.codigo || !o.rotulo) || !opcoes.some((o) => o.representaDesconhecido)}><Save /> Salvar caso</Button></div>
      </div>
    </div>
  </Section>;
}
