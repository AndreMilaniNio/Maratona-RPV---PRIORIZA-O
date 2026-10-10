import type { FieldValues, Path, UseFormSetError } from 'react-hook-form';
import { ApiError } from '@/services/api/client';

/**
 * Leva os erros por campo de um 400 (ProblemDetails.errors) para o formulário.
 * Chaves aninhadas ("integrantes[0].nome") caem no campo raiz quando só ele existe.
 * `porMensagem` permite associar mensagens de regra de negócio (que a API devolve
 * só no `title`) a um campo, ex.: 409 "Já existe equipe com este código." → codigo.
 * Devolve `true` quando todos os erros foram exibidos nos campos.
 */
export function aplicarErrosDeCampo<T extends FieldValues>(
  err: unknown,
  setError: UseFormSetError<T>,
  campos: readonly string[],
  porMensagem: { padrao: RegExp; campo: Path<T> }[] = [],
): boolean {
  if (!(err instanceof ApiError)) return false;
  const entradas = Object.entries(err.fieldErrors);
  if (entradas.length === 0) {
    const regra = porMensagem.find((r) => r.padrao.test(err.message));
    if (regra && (err.status === 400 || err.status === 409)) {
      setError(regra.campo, { type: 'server', message: err.message });
      return true;
    }
    return false;
  }
  let todos = true;
  for (const [chave, msgs] of entradas) {
    const normal = chave.replace(/\[(\d+)\]/g, '.$1');
    const raiz = normal.split('.')[0];
    const alvo = campos.includes(normal) ? normal : campos.includes(raiz) ? raiz : null;
    if (alvo) setError(alvo as Path<T>, { type: 'server', message: msgs.join(' ') });
    else todos = false;
  }
  return todos;
}
