import * as React from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { Link } from 'react-router';
import { useForm } from 'react-hook-form';
import { CheckCircle2, Send } from 'lucide-react';
import { ErrorState, InlineAlert, LoadingState } from '@/components/feedback/states';
import { notificarErro } from '@/components/feedback/toast';
import { Button } from '@/components/ui/button';
import { CheckboxField } from '@/components/ui/checkbox';
import { Input, NativeSelect, Textarea } from '@/components/ui/input';
import { PageHeader, Section } from '@/components/layout/PageHeader';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { useCatalogo } from '@/hooks/useCatalogo';
import { ROTAS } from '@/app/config/navigation';
import { criarSolicitacaoSchema, faixaDeQuantidade, paraNovaSolicitacaoRequest, type SolicitacaoFormValues, valoresIniciais } from '@/features/solicitacoes/schemas/solicitacaoSchema';
import { solicitacaoService } from '@/features/solicitacoes/services/solicitacaoService';

function erroDeCampo(erros: Record<string, unknown>, campo: string): string | undefined {
  const e = erros[campo] as { message?: string } | undefined;
  return e?.message;
}

export default function NovaSolicitacaoPage() {
  const { municipioId, municipio } = useCidade();
  const catalogo = useCatalogo();
  const form = useForm<SolicitacaoFormValues>({ resolver: zodResolver(criarSolicitacaoSchema()), defaultValues: valoresIniciais(undefined, municipioId) });
  const resultado = useMutation({
    mutationFn: (valores: SolicitacaoFormValues) => solicitacaoService.registrar(paraNovaSolicitacaoRequest(valores, crypto.randomUUID())),
    onError: (erro) => notificarErro(erro, 'Nao foi possivel registrar a solicitacao'),
  });

  React.useEffect(() => {
    if (!catalogo.data) return;
    form.reset(valoresIniciais(catalogo.data, municipioId));
  }, [catalogo.data, municipioId, form]);

  if (catalogo.isLoading) return <LoadingState label="Carregando formulario..." />;
  if (catalogo.error) return <ErrorState error={catalogo.error} onRetry={() => void catalogo.refetch()} title="Nao foi possivel carregar o formulario" />;
  if (!catalogo.data) return null;

  const { register, handleSubmit, watch, setValue, formState: { errors } } = form;
  const tipoSelecionado = watch('tipoOcorrenciaId');
  const naoSabeUc = watch('ucNaoInformada');
  const quantidadeUcs = watch('impacto.quantidadeUcs');
  const tipos = catalogo.data.tiposOcorrencia.filter((tipo) => tipo.ativo);

  return <div className="space-y-3">
    <PageHeader title="Nova solicitacao" description={`A OS sera criada e classificada no servidor para ${municipio?.nome ?? 'o municipio selecionado'}.`} />
    {resultado.data ? <Section title="Solicitacao registrada"><InlineAlert tone="success" title="OS criada e classificada" icon={<CheckCircle2 />}><p>Solicitacao {resultado.data.solicitacaoNumero} gerou a OS {resultado.data.osNumero}, com prioridade {resultado.data.prioridade?.nome} e {resultado.data.pontuacao} pontos.</p><Link className="mt-2 inline-block underline" to={ROTAS.os(resultado.data.osId)}>Abrir detalhes da OS</Link></InlineAlert></Section> : null}
    <form className="space-y-3" onSubmit={handleSubmit((v) => resultado.mutate(v))} noValidate>
      <Section title="Identificacao"><div className="grid gap-3 md:grid-cols-2">
        <label className="grid gap-1 text-[13px]"><span>Municipio</span><Input value={municipio?.nome ?? ''} readOnly aria-readonly /></label>
        <label className="grid gap-1 text-[13px]"><span>Tipo de ocorrencia</span><NativeSelect value={tipoSelecionado ?? ''} onChange={(e) => { const id = Number(e.target.value) || null; setValue('tipoOcorrenciaId', id, { shouldValidate: true }); const tipo = tipos.find((x) => x.id === id); if (tipo) setValue('tipoManutencao', tipo.tipoManutencaoSugerido, { shouldValidate: true }); }}><option value="">Selecione</option>{tipos.map((tipo) => <option key={tipo.id} value={tipo.id}>{tipo.nome}</option>)}</NativeSelect>{erroDeCampo(errors, 'tipoOcorrenciaId') && <span className="text-xs text-danger">{erroDeCampo(errors, 'tipoOcorrenciaId')}</span>}</label>
        <label className="grid gap-1 text-[13px]"><span>Manutencao</span><NativeSelect {...register('tipoManutencao')}><option value="">Selecione</option><option value="Corretiva">Corretiva</option><option value="Preventiva">Preventiva</option></NativeSelect></label>
        <label className="grid gap-1 text-[13px]"><span>Canal</span><NativeSelect {...register('canal')}><option value="Telefone">Telefone</option><option value="SistemaInterno">Sistema interno</option><option value="Presencial">Presencial</option><option value="Integracao">Integracao</option></NativeSelect></label>
      </div></Section>
      <Section title="Localizacao e UC" description="A UC pode ficar desconhecida, desde que exista uma forma de localizar a ocorrencia."><div className="grid gap-3 md:grid-cols-2">
        <label className="grid gap-1 text-[13px]"><span>Rua / logradouro</span><Input {...register('localizacao.logradouro')} placeholder="Rua, avenida ou referencia" /></label>
        <label className="grid gap-1 text-[13px]"><span>Numero</span><Input {...register('localizacao.numero')} /></label>
        <label className="grid gap-1 text-[13px]"><span>CEP</span><Input {...register('localizacao.cep')} inputMode="numeric" /></label>
        {!naoSabeUc && <label className="grid gap-1 text-[13px]"><span>Unidade consumidora</span><Input {...register('ucs.0.numero')} inputMode="numeric" placeholder="Numero da UC" /></label>}
        <label className="grid gap-1 text-[13px]"><span>Clientes/UCs atingidos na area</span><Input type="number" min="0" step="1" inputMode="numeric" value={quantidadeUcs ?? ''} onChange={(e) => { const valor = e.target.value === '' ? null : Number(e.target.value); setValue('impacto.quantidadeUcs', valor, { shouldValidate: true }); setValue('impacto.ucsAfetadas', valor === null || !Number.isFinite(valor) ? 'DESCONHECIDA' : faixaDeQuantidade(valor), { shouldValidate: true }); }} placeholder="Ex.: 45" /><span className="text-xs text-muted">Este total tem peso no cálculo da prioridade.</span></label>
        <CheckboxField id="uc-desconhecida" label="Solicitante nao sabe a UC" checked={naoSabeUc} onCheckedChange={(v) => setValue('ucNaoInformada', v, { shouldValidate: true })} />
        {naoSabeUc && <label className="grid gap-1 text-[13px]"><span>Motivo</span><Input {...register('motivoUcNaoInformada')} placeholder="Ex.: solicitante fora do local" /></label>}
      </div></Section>
      {catalogo.data.criteriosPersonalizados.length > 0 && <Section title="Casos configurados" description="Estas perguntas foram criadas no Construtor de Casos e suas respostas entram no calculo de prioridade."><div className="grid gap-3 md:grid-cols-2">{catalogo.data.criteriosPersonalizados.map((caso, indice) => <label key={caso.id} className="grid gap-1 text-[13px]"><span>{caso.nome}</span><NativeSelect {...register(`respostasPersonalizadas.${indice}.opcaoId`, { valueAsNumber: true })}><option value="">Nao informado</option>{caso.opcoes.map((opcao) => <option key={opcao.id} value={opcao.id}>{opcao.rotulo}</option>)}</NativeSelect><input type="hidden" {...register(`respostasPersonalizadas.${indice}.criterioId`, { valueAsNumber: true })} value={caso.id} /></label>)}</div></Section>}
      <Section title="Descricao da ocorrencia"><label className="grid gap-1 text-[13px]"><span>Descreva o fato observado</span><Textarea {...register('descricao')} placeholder="Informe o que ocorreu, impactos e detalhes conhecidos." /><span className="text-xs text-danger">{erroDeCampo(errors, 'descricao')}</span></label></Section>
      <div className="flex justify-end"><Button type="submit" size="lg" loading={resultado.isPending}><Send /> Registrar e gerar OS</Button></div>
    </form>
  </div>;
}
