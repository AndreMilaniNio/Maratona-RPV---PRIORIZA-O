import * as React from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField } from '@/components/forms/FormField';
import { Input, NativeSelect } from '@/components/ui/input';
import { notificarSucesso } from '@/components/feedback/toast';
import { DigitosInput } from '../comum/DigitosInput';
import type { LocalidadeDto, MunicipioDto } from '@/types/api';
import { FormDialog } from '../comum/FormDialog';
import { OpcoesMunicipio } from '../comum/FiltroMunicipio';
import { useSalvarLocalidade } from '../../hooks/useCadastros';
import { localidadeParaRequest, localidadeSchema, type LocalidadeForm } from '../../schemas/redeSchemas';
import { aplicarErrosApi } from '../../lib/formUtils';

const CAMPOS = ['codigo', 'nome', 'municipioId'] as const;

export function LocalidadeFormDialog({
  localidade,
  municipioPadrao,
  municipios,
  onClose,
}: {
  localidade: LocalidadeDto | null;
  municipioPadrao: number | null;
  municipios: readonly MunicipioDto[];
  onClose: () => void;
}) {
  const salvar = useSalvarLocalidade();
  const [erro, setErro] = React.useState<unknown>(null);
  const form = useForm<LocalidadeForm>({
    resolver: zodResolver(localidadeSchema),
    defaultValues: {
      codigo: localidade?.codigo ?? '',
      nome: localidade?.nome ?? '',
      municipioId: localidade?.municipioId ?? municipioPadrao ?? Number.NaN,
    },
  });
  const { register, formState: { errors } } = form;

  const onSubmit = form.handleSubmit(async (dados) => {
    setErro(null);
    try {
      await salvar.mutateAsync({ id: localidade?.id ?? null, dados: localidadeParaRequest(dados) });
      notificarSucesso(localidade ? 'Localidade atualizada.' : 'Localidade cadastrada.', `${dados.codigo} — ${dados.nome}`);
      onClose();
    } catch (e) {
      if (!aplicarErrosApi(e, form.setError, CAMPOS, 'codigo')) setErro(e);
    }
  });

  return (
    <FormDialog
      title={localidade ? `Editar localidade ${localidade.codigo ?? ''}` : 'Nova localidade'}
      description="Os 3 primeiros dígitos do número do transformador identificam a localidade. Não invente códigos: use os da norma operacional."
      size="sm"
      onClose={onClose}
      onSubmit={onSubmit}
      salvando={salvar.isPending}
      erro={erro}
    >
      <Controller
        control={form.control}
        name="codigo"
        render={({ field }) => (
          <FormField id="loc-codigo" label="Código (3 dígitos)" required error={errors.codigo?.message} hint="Texto: zeros à esquerda são preservados (ex.: 007).">
            <DigitosInput {...field} maxDigitos={3} className="w-24 font-mono" autoFocus />
          </FormField>
        )}
      />
      <FormField id="loc-nome" label="Nome" required error={errors.nome?.message}>
        <Input {...register('nome')} />
      </FormField>
      <FormField id="loc-municipio" label="Município" required error={errors.municipioId?.message}>
        <NativeSelect {...register('municipioId', { valueAsNumber: true })}>
          <option value="">Selecione…</option>
          <OpcoesMunicipio municipios={municipios} />
        </NativeSelect>
      </FormField>
    </FormDialog>
  );
}
