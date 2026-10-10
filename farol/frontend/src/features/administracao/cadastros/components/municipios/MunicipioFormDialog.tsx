import * as React from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField } from '@/components/forms/FormField';
import { Input } from '@/components/ui/input';
import { CheckboxField } from '@/components/ui/checkbox';
import { notificarSucesso } from '@/components/feedback/toast';
import type { MunicipioDto } from '@/types/api';
import { FormDialog } from '../comum/FormDialog';
import { useSalvarMunicipio } from '../../hooks/useCadastros';
import { municipioParaForm, municipioParaRequest, municipioSchema, type MunicipioForm } from '../../schemas/redeSchemas';
import { aplicarErrosApi } from '../../lib/formUtils';

const CAMPOS = ['nome', 'uf', 'codigoIbge', 'prefixo', 'latitude', 'longitude', 'raioKm', 'cepUnico', 'ativo'] as const;

export function MunicipioFormDialog({ municipio, onClose }: { municipio: MunicipioDto | null; onClose: () => void }) {
  const salvar = useSalvarMunicipio();
  const [erro, setErro] = React.useState<unknown>(null);
  const form = useForm<MunicipioForm>({ resolver: zodResolver(municipioSchema), defaultValues: municipioParaForm(municipio) });
  const { register, formState: { errors } } = form;

  const onSubmit = form.handleSubmit(async (dados) => {
    setErro(null);
    try {
      await salvar.mutateAsync({ id: municipio?.id ?? null, dados: municipioParaRequest(dados) });
      notificarSucesso(municipio ? 'Município atualizado.' : 'Município cadastrado.', dados.nome);
      onClose();
    } catch (e) {
      if (!aplicarErrosApi(e, form.setError, CAMPOS, 'prefixo')) setErro(e);
    }
  });

  return (
    <FormDialog
      title={municipio ? `Editar município — ${municipio.nome ?? ''}` : 'Novo município'}
      description="Coordenadas e raio de referência são usados para alertar localizações fora da cidade (não bloqueiam)."
      onClose={onClose}
      onSubmit={onSubmit}
      salvando={salvar.isPending}
      erro={erro}
    >
      <div className="grid gap-3 sm:grid-cols-[1fr_80px_120px]">
        <FormField id="mun-nome" label="Nome" required error={errors.nome?.message}>
          <Input {...register('nome')} autoFocus />
        </FormField>
        <FormField id="mun-uf" label="UF" required error={errors.uf?.message}>
          <Input {...register('uf')} maxLength={2} className="uppercase" />
        </FormField>
        <FormField id="mun-prefixo" label="Prefixo" required error={errors.prefixo?.message} hint="Usado na numeração das OS.">
          <Input {...register('prefixo', { setValueAs: (v: string) => v.toUpperCase() })} maxLength={6} className="uppercase" />
        </FormField>
      </div>
      <div className="grid gap-3 sm:grid-cols-2">
        <FormField id="mun-ibge" label="Código IBGE" error={errors.codigoIbge?.message} hint="Quando conhecido (7 dígitos).">
          <Input {...register('codigoIbge')} inputMode="numeric" maxLength={7} />
        </FormField>
        <FormField id="mun-cep" label="CEP único" error={errors.cepUnico?.message} hint="Só para cidades com um único CEP.">
          <Input {...register('cepUnico')} inputMode="numeric" maxLength={9} placeholder="00000000" />
        </FormField>
      </div>
      <div className="grid gap-3 sm:grid-cols-3">
        <FormField id="mun-lat" label="Latitude" error={errors.latitude?.message}>
          <Input {...register('latitude')} inputMode="decimal" placeholder="-21.5275" />
        </FormField>
        <FormField id="mun-lng" label="Longitude" error={errors.longitude?.message}>
          <Input {...register('longitude')} inputMode="decimal" placeholder="-42.6367" />
        </FormField>
        <FormField id="mun-raio" label="Raio (km)" error={errors.raioKm?.message}>
          <Input {...register('raioKm')} inputMode="decimal" />
        </FormField>
      </div>
      <Controller
        control={form.control}
        name="ativo"
        render={({ field }) => (
          <CheckboxField
            id="mun-ativo"
            label="Ativo"
            description="Município inativo não aparece para novas solicitações; o histórico é preservado."
            checked={field.value}
            onCheckedChange={field.onChange}
          />
        )}
      />
    </FormDialog>
  );
}
