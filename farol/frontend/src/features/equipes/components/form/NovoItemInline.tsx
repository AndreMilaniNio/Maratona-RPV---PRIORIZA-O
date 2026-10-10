import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { FormField } from '@/components/forms/FormField';
import { ErrorState } from '@/components/feedback/states';
import { notificarSucesso } from '@/components/feedback/toast';
import type { ItemCodigoDto } from '@/types/api';
import { useCriarItemEquipe, type TipoItemEquipe } from '@/features/equipes/hooks/useEquipes';
import { itemEquipeSchema, type ItemEquipeFormValues } from '@/features/equipes/schemas/equipeSchemas';
import { aplicarErrosDeCampo } from '@/features/equipes/utils/errosFormulario';

const NOME_TIPO: Record<TipoItemEquipe, string> = { qualificacao: 'qualificação', recurso: 'recurso' };
const MSG_CRIADO: Record<TipoItemEquipe, string> = { qualificacao: 'Qualificação cadastrada', recurso: 'Recurso cadastrado' };

/**
 * Cadastro rápido de qualificação/recurso dentro do formulário da equipe.
 * Não usa <form> (estaria aninhado no formulário da equipe): o envio é por botão.
 */
export function NovoItemInline({
  tipo,
  onCriado,
  onCancelar,
}: {
  tipo: TipoItemEquipe;
  onCriado: (item: ItemCodigoDto) => void;
  onCancelar: () => void;
}) {
  const criar = useCriarItemEquipe(tipo);
  const form = useForm<ItemEquipeFormValues>({ resolver: zodResolver(itemEquipeSchema), defaultValues: { codigo: '', nome: '' } });
  const { errors } = form.formState;
  const idBase = `novo-${tipo}`;

  const salvar = form.handleSubmit(async (v) => {
    try {
      const item = await criar.mutateAsync({ codigo: v.codigo, nome: v.nome });
      notificarSucesso(MSG_CRIADO[tipo], `${item.codigo} — ${item.nome}`);
      onCriado(item);
    } catch (e) {
      aplicarErrosDeCampo(e, form.setError, ['codigo', 'nome'], [{ padrao: /código/i, campo: 'codigo' }]);
    }
  });

  return (
    <div
      className="mt-2 space-y-2 rounded-md border border-dashed border-primary/40 bg-primary-soft/40 p-2.5"
      onKeyDown={(e) => {
        // Enter aqui não deve enviar o formulário da equipe.
        if (e.key === 'Enter' && e.target instanceof HTMLInputElement) {
          e.preventDefault();
          e.stopPropagation();
          void salvar();
        }
      }}
    >
      <p className="text-xs font-medium text-navy">Cadastrar {NOME_TIPO[tipo]} no catálogo</p>
      <div className="grid gap-2 sm:grid-cols-[180px_1fr]">
        <FormField id={`${idBase}-codigo`} label="Código" required error={errors.codigo?.message}>
          <Input
            {...form.register('codigo', { setValueAs: (v: string) => v.toUpperCase().replace(/\s+/g, '_') })}
            placeholder={tipo === 'qualificacao' ? 'EX.: PODA_ARVORES' : 'EX.: GUINDAUTO'}
            autoComplete="off"
          />
        </FormField>
        <FormField id={`${idBase}-nome`} label="Nome" required error={errors.nome?.message}>
          <Input {...form.register('nome')} autoComplete="off" />
        </FormField>
      </div>
      {criar.error && !form.formState.errors.codigo && <ErrorState error={criar.error} compact />}
      <div className="flex justify-end gap-2">
        <Button type="button" size="sm" variant="ghost" onClick={onCancelar} disabled={criar.isPending}>
          Cancelar
        </Button>
        <Button type="button" size="sm" onClick={() => void salvar()} loading={criar.isPending}>
          Cadastrar e selecionar
        </Button>
      </div>
    </div>
  );
}
