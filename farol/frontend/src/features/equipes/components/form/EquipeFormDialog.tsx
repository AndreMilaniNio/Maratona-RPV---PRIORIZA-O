import * as React from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input, NativeSelect } from '@/components/ui/input';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Dialog, DialogBody, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { FieldGroup, FormField } from '@/components/forms/FormField';
import { ErrorState, LoadingState } from '@/components/feedback/states';
import { notificarSucesso } from '@/components/feedback/toast';
import { useCatalogo } from '@/hooks/useCatalogo';
import { useCidade } from '@/features/autenticacao/hooks/CidadeProvider';
import { STATUS_EQUIPE_LABEL } from '@/lib/labels';
import { StatusEquipeValores, type EquipeDto } from '@/types/api';
import { useSalvarEquipe } from '@/features/equipes/hooks/useEquipes';
import {
  CAMPOS_EQUIPE_FORM,
  equipeFormInicial,
  equipeFormSchema,
  paraEquipeRequest,
  type EquipeFormValues,
} from '@/features/equipes/schemas/equipeSchemas';
import { aplicarErrosDeCampo } from '@/features/equipes/utils/errosFormulario';
import { ChecklistMultipla } from './ChecklistMultipla';
import { IntegrantesField } from './IntegrantesField';
import { ItensCatalogoField } from './ItensCatalogoField';

const numeroOuNaN = (v: unknown) => (v === '' || v === null || v === undefined ? Number.NaN : Number(v));

/** Cadastro (equipe = null) e edição de equipe — exige `equipes.administrar`. */
export function EquipeFormDialog({
  open,
  onOpenChange,
  equipe,
  onSalvo,
}: {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  equipe: EquipeDto | null;
  onSalvo?: (e: EquipeDto) => void;
}) {
  const catalogo = useCatalogo();
  const { municipioId } = useCidade();
  const salvar = useSalvarEquipe(equipe?.id ?? null);
  const form = useForm<EquipeFormValues>({
    resolver: zodResolver(equipeFormSchema),
    defaultValues: equipeFormInicial(equipe, municipioId),
  });
  const { register, control, formState, reset, setError, watch } = form;
  const errors = formState.errors;
  const [erroGeral, setErroGeral] = React.useState<unknown>(null);

  React.useEffect(() => {
    if (open) {
      reset(equipeFormInicial(equipe, municipioId));
      setErroGeral(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, equipe?.id]);

  const municipios = catalogo.data?.municipios ?? [];
  const baseId = watch('municipioBaseId');

  const enviar = form.handleSubmit(async (v) => {
    setErroGeral(null);
    try {
      const salva = await salvar.mutateAsync(paraEquipeRequest(v));
      notificarSucesso(equipe ? 'Equipe atualizada' : 'Equipe cadastrada', `${salva.codigo} — ${salva.nome}`);
      onOpenChange(false);
      onSalvo?.(salva);
    } catch (e) {
      const tudoNosCampos = aplicarErrosDeCampo(e, setError, CAMPOS_EQUIPE_FORM, [
        { padrao: /código/i, campo: 'codigo' },
        { padrao: /capacidade/i, campo: 'capacidade' },
        { padrao: /integrante/i, campo: 'integrantes' },
      ]);
      if (!tudoNosCampos) setErroGeral(e);
    }
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent size="xl">
        <DialogHeader>
          <DialogTitle>{equipe ? `Editar equipe ${equipe.codigo}` : 'Cadastrar equipe'}</DialogTitle>
          <DialogDescription>
            Dados operacionais da equipe. A disponibilidade para despacho é calculada pelo servidor (status, capacidade e despachos ativos).
          </DialogDescription>
        </DialogHeader>
        {catalogo.isLoading ? (
          <DialogBody>
            <LoadingState label="Carregando catálogo…" />
          </DialogBody>
        ) : catalogo.isError ? (
          <DialogBody>
            <ErrorState error={catalogo.error} onRetry={() => void catalogo.refetch()} />
          </DialogBody>
        ) : (
          <form id="form-equipe" onSubmit={(e) => void enviar(e)} noValidate className="contents">
            <DialogBody className="space-y-4">
              <div className="grid gap-3 sm:grid-cols-[160px_1fr_200px]">
                <FormField id="equipe-codigo" label="Código operacional" required error={errors.codigo?.message} hint="Ex.: LUM-05">
                  <Input {...register('codigo')} autoComplete="off" />
                </FormField>
                <FormField id="equipe-nome" label="Nome" required error={errors.nome?.message}>
                  <Input {...register('nome')} autoComplete="off" />
                </FormField>
                <FormField id="equipe-status" label="Status" required error={errors.status?.message}>
                  <NativeSelect {...register('status')}>
                    {StatusEquipeValores.map((s) => (
                      <option key={s} value={s}>
                        {STATUS_EQUIPE_LABEL[s]}
                      </option>
                    ))}
                  </NativeSelect>
                </FormField>
              </div>
              <div className="grid gap-3 sm:grid-cols-[1fr_200px_auto]">
                <FormField id="equipe-municipio-base" label="Município-base" required error={errors.municipioBaseId?.message}>
                  <NativeSelect {...register('municipioBaseId', { setValueAs: numeroOuNaN })}>
                    <option value="">Selecione…</option>
                    {municipios.map((m) => (
                      <option key={m.id} value={m.id}>
                        {m.nome}
                        {m.prefixo ? ` (${m.prefixo})` : ''}
                      </option>
                    ))}
                  </NativeSelect>
                </FormField>
                <FormField
                  id="equipe-capacidade"
                  label="Capacidade"
                  required
                  error={errors.capacidade?.message}
                  hint="Despachos simultâneos (1 a 10)"
                >
                  <Input type="number" min={1} max={10} step={1} {...register('capacidade', { setValueAs: numeroOuNaN })} />
                </FormField>
                <div className="flex flex-col gap-1">
                  <Label htmlFor="equipe-ativa">Ativa</Label>
                  <Controller
                    control={control}
                    name="ativa"
                    render={({ field }) => (
                      <div className="flex h-8 items-center gap-2">
                        <Switch id="equipe-ativa" checked={field.value} onCheckedChange={field.onChange} />
                        <span className="text-[13px] text-muted">{field.value ? 'Sim' : 'Não'}</span>
                      </div>
                    )}
                  />
                </div>
              </div>
              <Controller
                control={control}
                name="municipiosAdicionaisIds"
                render={({ field }) => (
                  <FieldGroup
                    legend="Municípios adicionais"
                    description="Cidades em que a equipe também pode atuar. Fora delas, só como apoio intermunicipal com justificativa."
                    error={errors.municipiosAdicionaisIds?.message}
                  >
                    <ChecklistMultipla
                      idBase="equipe-mun-adicional"
                      colunas={3}
                      opcoes={municipios.map((m) => ({
                        id: m.id,
                        rotulo: m.nome ?? String(m.id),
                        desabilitada: m.id === baseId,
                        descricao: m.id === baseId ? 'Município-base' : null,
                      }))}
                      value={field.value.filter((id) => id !== baseId)}
                      onChange={field.onChange}
                    />
                  </FieldGroup>
                )}
              />
              <IntegrantesField control={control} register={register} errors={errors} />
              <div className="grid gap-4 lg:grid-cols-2">
                <Controller
                  control={control}
                  name="qualificacoesIds"
                  render={({ field }) => (
                    <ItensCatalogoField
                      tipo="qualificacao"
                      legend="Qualificações"
                      description="Usadas na compatibilidade com o tipo de ocorrência."
                      itens={catalogo.data?.qualificacoes ?? []}
                      value={field.value}
                      onChange={field.onChange}
                      error={errors.qualificacoesIds?.message}
                    />
                  )}
                />
                <Controller
                  control={control}
                  name="recursosIds"
                  render={({ field }) => (
                    <ItensCatalogoField
                      tipo="recurso"
                      legend="Recursos e equipamentos"
                      itens={catalogo.data?.recursos ?? []}
                      value={field.value}
                      onChange={field.onChange}
                      error={errors.recursosIds?.message}
                    />
                  )}
                />
              </div>
              {erroGeral != null && <ErrorState error={erroGeral} title="Não foi possível salvar a equipe" />}
            </DialogBody>
            <DialogFooter>
              <Button type="button" variant="secondary" onClick={() => onOpenChange(false)} disabled={salvar.isPending}>
                Cancelar
              </Button>
              <Button type="submit" loading={salvar.isPending}>
                {equipe ? 'Salvar alterações' : 'Cadastrar equipe'}
              </Button>
            </DialogFooter>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
}
