import { useFieldArray, type Control, type FieldErrors, type UseFormRegister } from 'react-hook-form';
import { Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { FieldGroup, FormField } from '@/components/forms/FormField';
import type { EquipeFormValues } from '@/features/equipes/schemas/equipeSchemas';

/** Lista dinâmica de integrantes (nome obrigatório; matrícula e função opcionais). */
export function IntegrantesField({
  control,
  register,
  errors,
}: {
  control: Control<EquipeFormValues>;
  register: UseFormRegister<EquipeFormValues>;
  errors: FieldErrors<EquipeFormValues>;
}) {
  const { fields, append, remove } = useFieldArray({ control, name: 'integrantes' });
  const erroLista = errors.integrantes?.message ?? errors.integrantes?.root?.message;
  return (
    <FieldGroup legend="Integrantes" description="Ao menos um integrante. Informe a função (ex.: Encarregado, Eletricista)." error={erroLista}>
      <ol className="space-y-2">
        {fields.map((f, i) => {
          const e = errors.integrantes?.[i];
          return (
            <li key={f.id} className="grid items-start gap-2 rounded-md border border-line bg-white p-2 sm:grid-cols-[1fr_140px_160px_auto]">
              <FormField id={`integrante-${i}-nome`} label={`Nome do integrante ${i + 1}`} required error={e?.nome?.message}>
                <Input {...register(`integrantes.${i}.nome`)} autoComplete="off" />
              </FormField>
              <FormField id={`integrante-${i}-matricula`} label="Matrícula" error={e?.matricula?.message}>
                <Input {...register(`integrantes.${i}.matricula`)} autoComplete="off" />
              </FormField>
              <FormField id={`integrante-${i}-funcao`} label="Função" error={e?.funcao?.message}>
                <Input {...register(`integrantes.${i}.funcao`)} autoComplete="off" list="equipe-funcoes-sugeridas" />
              </FormField>
              <Button
                type="button"
                variant="ghost"
                size="icon-sm"
                className="sm:mt-6"
                onClick={() => remove(i)}
                disabled={fields.length <= 1}
                aria-label={`Remover integrante ${i + 1}`}
                title={fields.length <= 1 ? 'A equipe precisa de ao menos um integrante' : 'Remover integrante'}
              >
                <Trash2 />
              </Button>
            </li>
          );
        })}
      </ol>
      <datalist id="equipe-funcoes-sugeridas">
        <option value="Encarregado" />
        <option value="Eletricista" />
        <option value="Motorista" />
        <option value="Técnico" />
        <option value="Auxiliar" />
      </datalist>
      <Button type="button" variant="secondary" size="sm" className="mt-2" onClick={() => append({ nome: '', matricula: '', funcao: '' })}>
        <Plus /> Adicionar integrante
      </Button>
    </FieldGroup>
  );
}
