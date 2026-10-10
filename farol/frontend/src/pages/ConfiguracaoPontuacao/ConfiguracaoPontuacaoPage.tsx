import * as React from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, FilePlus2, History, Save, Send } from 'lucide-react';
import { ErrorState, InlineAlert, LoadingState } from '@/components/feedback/states';
import { notificarErro, notificarSucesso } from '@/components/feedback/toast';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { CheckboxField } from '@/components/ui/checkbox';
import { Input, NativeSelect, Textarea } from '@/components/ui/input';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { ConstrutorCasos } from '@/features/priorizacao/components/ConstrutorCasos';
import { pontuacaoService } from '@/services/api/pontuacaoService';
import { queryKeys } from '@/services/api/queryKeys';
import type { AplicacaoVersao, VersaoDetalheDto } from '@/types/api';

type Edicao = {
  criterios: Record<number, boolean>;
  pontos: Record<number, { pontos: string; requerConfirmacao: boolean }>;
  faixas: Record<number, { minimo: string; maximo: string }>;
};

function criarEdicao(versao: VersaoDetalheDto): Edicao {
  return {
    criterios: Object.fromEntries(versao.criterios.map((c) => [c.criterioId, c.habilitado])),
    pontos: Object.fromEntries(versao.criterios.flatMap((c) => c.opcoes.map((o) => [o.opcaoId, { pontos: o.pontos?.toString() ?? '', requerConfirmacao: o.requerConfirmacao }]))),
    faixas: Object.fromEntries(versao.faixas.map((f) => [f.prioridadeId, { minimo: f.minimo?.toString() ?? '0', maximo: f.maximo?.toString() ?? '' }])),
  };
}

function inteiro(valor: string): number | null {
  const texto = valor.trim();
  if (!texto) return null;
  const n = Number(texto);
  return Number.isInteger(n) ? n : null;
}

export default function ConfiguracaoPontuacaoPage() {
  const { municipioId, municipio } = useCidade();
  const queryClient = useQueryClient();
  const [edicao, setEdicao] = React.useState<Edicao | null>(null);
  const [justificativa, setJustificativa] = React.useState('');
  const [aplicacao, setAplicacao] = React.useState<AplicacaoVersao>('SomenteNovas');
  const configuracao = useQuery({ queryKey: queryKeys.pontuacao(municipioId), queryFn: () => pontuacaoService.obter(municipioId) });
  const versoes = useQuery({ queryKey: queryKeys.pontuacaoVersoes(municipioId), queryFn: () => pontuacaoService.versoes(municipioId) });
  const rascunho = configuracao.data?.rascunho ?? null;

  React.useEffect(() => {
    setEdicao(rascunho ? criarEdicao(rascunho) : null);
    setJustificativa('');
  }, [rascunho?.id, rascunho?.token]);

  const invalidar = React.useCallback(() => {
    void queryClient.invalidateQueries({ queryKey: queryKeys.pontuacao(municipioId) });
    void queryClient.invalidateQueries({ queryKey: queryKeys.pontuacaoVersoes(municipioId) });
  }, [municipioId, queryClient]);
  const criar = useMutation({
    mutationFn: () => pontuacaoService.criarRascunho(municipioId),
    onSuccess: () => { notificarSucesso('Rascunho criado.'); invalidar(); },
    onError: (erro) => notificarErro(erro, 'Nao foi possivel criar o rascunho'),
  });
  const salvar = useMutation({
    mutationFn: () => {
      if (!rascunho || !edicao) throw new Error('Nao ha rascunho para salvar.');
      return pontuacaoService.salvarRascunho({
        versaoId: rascunho.id,
        token: rascunho.token,
        criterios: Object.entries(edicao.criterios).map(([criterioId, habilitado]) => ({ criterioId: Number(criterioId), habilitado })),
        pontos: Object.entries(edicao.pontos).map(([opcaoId, p]) => ({ opcaoId: Number(opcaoId), pontos: inteiro(p.pontos), requerConfirmacao: p.requerConfirmacao })),
        faixas: Object.entries(edicao.faixas).map(([prioridadeId, f]) => ({ prioridadeId: Number(prioridadeId), minimo: inteiro(f.minimo) ?? 0, maximo: inteiro(f.maximo) })),
      });
    },
    onSuccess: () => { notificarSucesso('Rascunho salvo.'); invalidar(); },
    onError: (erro) => notificarErro(erro, 'Nao foi possivel salvar o rascunho'),
  });
  const publicar = useMutation({
    mutationFn: () => {
      if (!rascunho) throw new Error('Nao ha rascunho para publicar.');
      return pontuacaoService.publicar({ versaoId: rascunho.id, justificativa, aplicacao });
    },
    onSuccess: () => { notificarSucesso('Versao publicada.'); invalidar(); },
    onError: (erro) => notificarErro(erro, 'Nao foi possivel publicar a versao'),
  });

  const alterarPonto = (id: number, campo: 'pontos' | 'requerConfirmacao', valor: string | boolean) => {
    setEdicao((atual) => atual && ({ ...atual, pontos: { ...atual.pontos, [id]: { ...atual.pontos[id], [campo]: valor } } }));
  };
  const alterarFaixa = (id: number, campo: 'minimo' | 'maximo', valor: string) => {
    setEdicao((atual) => atual && ({ ...atual, faixas: { ...atual.faixas, [id]: { ...atual.faixas[id], [campo]: valor } } }));
  };

  if (configuracao.isLoading) return <LoadingState label="Carregando configuracao de prioridade..." />;
  if (configuracao.error) return <ErrorState error={configuracao.error} onRetry={() => void configuracao.refetch()} title="Nao foi possivel carregar a configuracao" />;
  if (!configuracao.data) return null;
  const dados = configuracao.data;
  const escopo = municipio?.nome ?? dados.escopo ?? 'todas as cidades';

  return (
    <div className="space-y-3">
      <PageHeader
        title="Formulario e pontuacao"
        description={`Configuracao demonstrativa de ${escopo}. A prioridade e calculada no servidor.`}
        actions={!rascunho && dados.podeEditar ? <Button onClick={() => criar.mutate()} loading={criar.isPending}><FilePlus2 /> Criar rascunho</Button> : undefined}
      />
      {municipioId === null && <InlineAlert tone="warning" title="Escopo global legado"><p>Selecione uma cidade no cabecalho para configurar um formulario municipal independente.</p></InlineAlert>}
      {dados.vigenteHerdadaDaGlobal && municipioId !== null && <InlineAlert tone="info" title="Usando configuracao global"><p>Este municipio ainda nao possui versao propria. Criar um rascunho gera a primeira versao municipal.</p></InlineAlert>}
      {dados.aguardandoAprovacao && <InlineAlert tone="warning" title="Versao aguardando aprovacao"><p>Ha uma versao pendente para este escopo.</p></InlineAlert>}
      {dados.podeEditar && <ConstrutorCasos municipioId={municipioId} />}

      {rascunho && edicao ? <>
        <Section title={`Rascunho v${rascunho.numero}`} description="Habilite criterios e defina pontos por resposta. A soma nao precisa ser 100." actions={<Button onClick={() => salvar.mutate()} loading={salvar.isPending}><Save /> Salvar rascunho</Button>}>
          <div className="space-y-4">
            {rascunho.criterios.map((criterio) => <div key={criterio.criterioId} className="rounded border border-line p-3">
              <CheckboxField
                id={`criterio-${criterio.criterioId}`}
                label={<span className="font-medium">{criterio.nome}</span>}
                checked={edicao.criterios[criterio.criterioId] ?? false}
                onCheckedChange={(habilitado) => setEdicao((atual) => atual && ({ ...atual, criterios: { ...atual.criterios, [criterio.criterioId]: habilitado } }))}
                disabled={!dados.podeEditar}
                description={criterio.descricao ?? (criterio.multiplaEscolha ? 'Multipla escolha.' : 'Uma resposta.')}
              />
              <div className="mt-3 overflow-x-auto"><table className="w-full min-w-[540px] text-left text-[13px]">
                <thead className="border-b border-line text-xs text-muted"><tr><th className="pb-2 font-medium">Resposta</th><th className="pb-2 font-medium">Pontos</th><th className="pb-2 font-medium">Confirmacao</th></tr></thead>
                <tbody>{criterio.opcoes.map((opcao) => <tr key={opcao.opcaoId} className="border-b border-line/70 last:border-0">
                  <td className="py-2">{opcao.rotulo} {opcao.representaDesconhecido && <Badge variant="warning">desconhecida</Badge>}</td>
                  <td className="py-2 pr-3"><Input aria-label={`Pontos de ${opcao.rotulo}`} inputMode="numeric" value={edicao.pontos[opcao.opcaoId]?.pontos ?? ''} onChange={(e) => alterarPonto(opcao.opcaoId, 'pontos', e.target.value)} disabled={!dados.podeEditar || !edicao.criterios[criterio.criterioId]} placeholder="nao definido" /></td>
                  <td className="py-2"><CheckboxField id={`confirmar-${opcao.opcaoId}`} label="Requer confirmacao" checked={edicao.pontos[opcao.opcaoId]?.requerConfirmacao ?? false} onCheckedChange={(v) => alterarPonto(opcao.opcaoId, 'requerConfirmacao', v)} disabled={!dados.podeEditar || !edicao.criterios[criterio.criterioId]} /></td>
                </tr>)}</tbody>
              </table></div>
            </div>)}
          </div>
        </Section>
        <Section title="Faixas de classificacao" description="As faixas devem cobrir de 0 ate o limite aberto, sem lacunas ou sobreposicoes.">
          <div className="overflow-x-auto"><table className="w-full min-w-[480px] text-left text-[13px]">
            <thead className="border-b border-line text-xs text-muted"><tr><th className="pb-2">Prioridade</th><th className="pb-2">Minimo</th><th className="pb-2">Maximo (vazio = sem limite)</th></tr></thead>
            <tbody>{rascunho.faixas.map((faixa) => <tr key={faixa.prioridadeId} className="border-b border-line/70 last:border-0"><td className="py-2"><span className="mr-2 inline-block h-2 w-2 rounded-full" style={{ background: faixa.cor ?? '#64748b' }} />{faixa.nome}</td><td className="py-2 pr-3"><Input inputMode="numeric" value={edicao.faixas[faixa.prioridadeId]?.minimo ?? ''} onChange={(e) => alterarFaixa(faixa.prioridadeId, 'minimo', e.target.value)} /></td><td className="py-2"><Input inputMode="numeric" value={edicao.faixas[faixa.prioridadeId]?.maximo ?? ''} onChange={(e) => alterarFaixa(faixa.prioridadeId, 'maximo', e.target.value)} /></td></tr>)}</tbody>
          </table></div>
        </Section>
        {(rascunho.pendencias.length > 0 || rascunho.alertas.length > 0) && <InlineAlert tone={rascunho.pendencias.length > 0 ? 'danger' : 'warning'} title={rascunho.pendencias.length > 0 ? 'Pendencias para publicar' : 'Alertas de coerencia'} icon={<AlertTriangle />}><ul className="list-disc pl-4">{[...rascunho.pendencias, ...rascunho.alertas].map((texto) => <li key={texto}>{texto}</li>)}</ul></InlineAlert>}
        <Section title="Publicar versao" description="A versao publicada preserva resultados anteriores e pode reclassificar OS abertas.">
          <div className="grid gap-3 md:grid-cols-[1fr_220px_auto] md:items-end">
            <label className="grid gap-1 text-[13px]"><span className="font-medium">Justificativa</span><Textarea value={justificativa} onChange={(e) => setJustificativa(e.target.value)} placeholder="Explique esta mudanca" /></label>
            <label className="grid gap-1 text-[13px]"><span className="font-medium">Aplicacao</span><NativeSelect value={aplicacao} onChange={(e) => setAplicacao(e.target.value as AplicacaoVersao)}><option value="SomenteNovas">Somente novas OS</option><option value="ReclassificarAbertas">Reclassificar OS abertas</option></NativeSelect></label>
            <Button onClick={() => publicar.mutate()} loading={publicar.isPending} disabled={!justificativa.trim() || rascunho.pendencias.length > 0 || !dados.podePublicar}><Send /> Publicar</Button>
          </div>
        </Section>
      </> : <InlineAlert tone="info" title="Nenhum rascunho aberto"><p>Crie um rascunho para editar a proxima versao sem alterar a versao vigente.</p></InlineAlert>}

      <Section title="Historico de versoes" icon={<History />} description="Versoes publicadas nao sao apagadas; cada OS mantem o calculo que recebeu.">
        {versoes.isLoading ? <LoadingState label="Carregando versoes..." /> : versoes.error ? <ErrorState error={versoes.error} compact onRetry={() => void versoes.refetch()} /> : <div className="overflow-x-auto"><table className="w-full min-w-[620px] text-left text-[13px]">
          <thead className="border-b border-line text-xs text-muted"><tr><th className="pb-2">Versao</th><th className="pb-2">Status</th><th className="pb-2">Vigencia</th><th className="pb-2">Aplicacao</th><th className="pb-2">Justificativa</th></tr></thead>
          <tbody>{versoes.data?.map((versao) => <tr key={versao.id} className="border-b border-line/70 last:border-0"><td className="py-2">v{versao.numero}{versao.emVigor && <Badge className="ml-2" variant="success">vigente</Badge>}</td><td className="py-2">{versao.status}</td><td className="py-2">{versao.vigenciaInicio ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(versao.vigenciaInicio)) : '-'}</td><td className="py-2">{versao.aplicacao === 'ReclassificarAbertas' ? 'Reclassifica abertas' : 'Somente novas'}</td><td className="py-2">{versao.justificativa ?? '-'}</td></tr>)}</tbody>
        </table></div>}
      </Section>
    </div>
  );
}
